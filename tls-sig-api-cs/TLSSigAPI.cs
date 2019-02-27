using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using zlib;

// 这里依赖了 zlib.net 类库，使用 nuget 或者 https://www.nuget.org/ 站点手动下载

// 代码中关于 ANS.1 代码参考了 .net core System.Security.Cryptography 下 DerEncoder.cs 和 DerSequenceReader.cs

namespace tencentyun
{
    public class TLSSigAPI
    {
        private ECDsaCng ecdsa;
        private int sdkappid;

        private static byte[] SHA256(string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            return Sha256.ComputeHash(SHA256Data);
        }

        private static byte[] compressBytes(byte[] sourceByte)
        {
            MemoryStream inputStream = new MemoryStream(sourceByte);
            Stream outStream = compressStream(inputStream);
            byte[] outPutByteArray = new byte[outStream.Length];
            outStream.Position = 0;
            outStream.Read(outPutByteArray, 0, outPutByteArray.Length);
            outStream.Close();
            inputStream.Close();
            return outPutByteArray;
        }

        private static Stream compressStream(Stream sourceStream)
        {
            MemoryStream streamOut = new MemoryStream();
            ZOutputStream streamZOut = new ZOutputStream(streamOut, zlibConst.Z_DEFAULT_COMPRESSION);
            CopyStream(sourceStream, streamZOut);
            streamZOut.finish();
            return streamOut;
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        private static byte[] EncodeLength(int length)
        {
            byte low = unchecked((byte)length);

            // If the length value fits in 7 bits, it's an answer all by itself.
            if (length < 0x80)
            {
                return new[] { low };
            }

            // If the length is more than 0x7F then it is stored as
            // 0x80 | lengthLength
            // big
            // endian
            // length

            // So:
            // 0 => 0x00.
            // 1 => 0x01.
            // 127 => 0x7F.
            // 128 => 0x81 0x80
            // 255 => 0x81 0xFF
            // 256 => 0x82 0x01 0x00
            // 65535 => 0x82 0xFF 0xFF
            // 65536 => 0x83 0x01 0x00 0x00
            // ...
            // int.MaxValue => 0x84 0x7F 0xFF 0xFF 0xFF
            //
            // Technically DER lengths can go longer than int.MaxValue, but since our
            // encoding input here is an int, our output will be no larger than that.

            if (length <= 0xFF)
            {
                return new byte[] { 0x81, low };
            }

            int remainder = length >> 8;
            byte midLow = unchecked((byte)remainder);

            if (length <= 0xFFFF)
            {
                return new byte[] { 0x82, midLow, low };
            }

            remainder >>= 8;
            byte midHigh = unchecked((byte)remainder);

            if (length <= 0xFFFFFF)
            {
                return new byte[] { 0x83, midHigh, midLow, low };
            }

            remainder >>= 8;
            byte high = unchecked((byte)remainder);

            // Since we know this was a non-negative signed number, the highest
            // legal value here is 0x7F.
            return new byte[] { 0x84, high, midHigh, midLow, low };
        }

        private static byte[][] SegmentedEncodeUnsignedInteger(byte[] bigEndianBytes, int offset, int count)
        {
            int start = offset;
            int end = start + count;

            // 去掉 0 前缀
            while (start < end && bigEndianBytes[start] == 0)
            {
                start++;
            }

            // 如果全为 0，那么保留一个 byte 即可
            if (start == end)
            {
                start--;
            }

            int length = end - start;
            byte[] dataBytes;
            int writeStart = 0;

            // 如果第一个字节是大于 0x7F 的，说明是负数，但是我们是无符号的，需要在最前面加 0，这样就不可能解析成负数了
            if (bigEndianBytes[start] > 0x7F)
            {
                dataBytes = new byte[length + 1];
                writeStart = 1;
            }
            else
            {
                dataBytes = new byte[length];
            }

            // 把数据全部放入缓冲区
            Buffer.BlockCopy(bigEndianBytes, start, dataBytes, writeStart, length);

            return new[]
            {
                new[] { (byte)0x02 }, EncodeLength(dataBytes.Length), dataBytes,
            };
        }

        private static byte[] ConstructSequence(IEnumerable<byte[][]> items)
        {
            // A more robust solution would be required for public API.  DerInteger(int), DerBoolean, etc,
            // which do not allow the user to specify lengths, but only the payload.  But for efficiency things
            // are tracked as just little segments of bytes, and they're not glued together until this method.

            int payloadLength = 0;

            foreach (byte[][] segments in items)
            {
                foreach (byte[] segment in segments)
                {
                    payloadLength += segment.Length;
                }
            }

            byte[] encodedLength = EncodeLength(payloadLength);

            // The tag (1) + the length of the length + the length of the payload
            byte[] encodedSequence = new byte[1 + encodedLength.Length + payloadLength];

            // constructed flg | sequence tag value
            encodedSequence[0] = 0x20 | 0x10;

            int writeStart = 1;

            Buffer.BlockCopy(encodedLength, 0, encodedSequence, writeStart, encodedLength.Length);

            writeStart += encodedLength.Length;

            foreach (byte[][] segments in items)
            {
                foreach (byte[] segment in segments)
                {
                    Buffer.BlockCopy(segment, 0, encodedSequence, writeStart, segment.Length);
                    writeStart += segment.Length;
                }
            }

            return encodedSequence;
        }

        public TLSSigAPI(int sdkappid, string priKeyContent, string pubKeyContent="")
        {
            ecdsa = new ECDsaCng(CngKey.Import(Convert.FromBase64String(priKeyContent), CngKeyBlobFormat.Pkcs8PrivateBlob));
            this.sdkappid = sdkappid;
        }

        // 默认使用 180 天有效期
        public string genSig(string identifier, int expireTime = 3600*24*180)
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            Int64 currTime = (Int64)(DateTime.UtcNow - epoch).TotalMilliseconds/1000;
            string rawData =
                "TLS.appid_at_3rd:" + 0 + "\n" +
                "TLS.account_type:" + 0 + "\n" +
                "TLS.identifier:" + identifier + "\n" +
                "TLS.sdk_appid:" + sdkappid + "\n" +
                "TLS.time:" + currTime + "\n" +
                "TLS.expire_after:" + expireTime + "\n";

            byte[] rawDataHash = SHA256(rawData);
            byte[] rawSig = ecdsa.SignHash(rawDataHash);

            // .net 接口生成的 sig 与 openssl 的不一致
            // 所以为了兼容需要吧 sig 进行转换
            int halfLength = rawSig.Length / 2;
            byte[][] rEncoded = SegmentedEncodeUnsignedInteger(rawSig, 0, halfLength);
            byte[][] sEncoded = SegmentedEncodeUnsignedInteger(rawSig, halfLength, halfLength);
            List<byte[][]> items = new List<byte[][]>() { rEncoded, sEncoded };
            byte[] opensslSig = ConstructSequence(items);
            string base64sig = Convert.ToBase64String(opensslSig);

            // 没有引入 json 库，所以这里手动进行组装
            string jsonData = String.Format("{{"
                + "\"TLS.account_type\":" + "\"0\","
                + "\"TLS.identifier\":" + "\"{0}\","
                + "\"TLS.appid_at_3rd\":" +  "\"0\","
                + "\"TLS.sdk_appid\":" + "\"{1}\","
                + "\"TLS.expire_after\":" + "\"{2}\","
                + "\"TLS.time\":" + "\"{3}\","
                + "\"TLS.sig\":" + "\"{4}\""
                + "}}", identifier, sdkappid, expireTime, currTime, base64sig);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
            // 下面的压缩调用 zlib.net 类库的接口，请予以引入
            return Convert.ToBase64String(compressBytes(buffer))
                .Replace('+', '*').Replace('/', '-').Replace('=', '_');
        }
    }
}

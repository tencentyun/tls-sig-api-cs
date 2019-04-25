using System;
using System.Text;
using System.IO;

using ComponentAce.Compression.Libs.zlib;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Signers;


// 这里依赖了 zlib.net-mutliplatform 和 Portable.BouncyCastle 类库，使用 nuget 或者 https://www.nuget.org/ 站点手动下载

namespace tencentyun
{
    class BC
    {
        DsaDigestSigner ecdsaSigner;

        public BC(string priKeyContent)
        {
            PemReader pemReader = new PemReader(new StringReader(priKeyContent));
            ECPrivateKeyParameters priKey = pemReader.ReadObject() as ECPrivateKeyParameters;
            ecdsaSigner = SignerUtilities.GetSigner("SHA-256withECDSA") as DsaDigestSigner;
            ecdsaSigner.Init(true, priKey);
        }

        public byte[] Sign(byte[] message)
        {
            ecdsaSigner.Reset();
            ecdsaSigner.BlockUpdate(message, 0, message.Length);
            return ecdsaSigner.GenerateSignature();
        }
    }

    public class TLSSigAPI
    {
        private BC bc;
        private int sdkappid;

        private static byte[] CompressBytes(byte[] sourceByte)
        {
            MemoryStream inputStream = new MemoryStream(sourceByte);
            Stream outStream = CompressStream(inputStream);
            byte[] outPutByteArray = new byte[outStream.Length];
            outStream.Position = 0;
            outStream.Read(outPutByteArray, 0, outPutByteArray.Length);
            //outStream.Close();
            //inputStream.Close();
            return outPutByteArray;
        }

        private static Stream CompressStream(Stream sourceStream)
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

        public TLSSigAPI(int sdkappid, string priKeyContent, string pubKeyContent="")
        {
            this.sdkappid = sdkappid;
            bc = new BC(priKeyContent);
        }

        // 默认使用 180 天有效期
        public string GenSig(string identifier, int expireTime = 3600*24*180)
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
            byte[] rawSig = bc.Sign(Encoding.UTF8.GetBytes(rawData));
            string base64sig = Convert.ToBase64String(rawSig);

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
            return Convert.ToBase64String(CompressBytes(buffer))
                .Replace('+', '*').Replace('/', '-').Replace('=', '_');
        }
    }
}

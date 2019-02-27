using System;
using tencentyun;

namespace tls_sig_api_cs
{
    class TLSSigAPIDemo
    {
        static public void Main(String[] args)
        {
            // .net 导入私钥的接口与 openssl 不一致
            // 这里私钥需要把下载到 pem 文件中的头尾去掉
            string privKeyContent = "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgkTfHxPa8YusG+va8"
                + "1CRztNQBOEr90TBEjlQBZ5d1Y0ChRANCAAS9isP/xLib7EZ1vS5OUy+gOsYBwees"
                + "PMDvWiTygPAUsGZv1PHLoa0ciqsElkO1fMGwNrzOKJx1Oo194Ri+SypV";
            TLSSigAPI api = new TLSSigAPI(1400000000, privKeyContent, "");
            Console.WriteLine(api.genSig("xiaojun"));
            Console.ReadKey();
        }
    }
}

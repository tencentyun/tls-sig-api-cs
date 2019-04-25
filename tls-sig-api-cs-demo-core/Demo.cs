using System;
using tencentyun;

namespace tls_sig_api_cs_demo_core
{
    class Demo
    {
        static void Main(string[] args)
        {
            // 这里私钥直接使用私钥文件中的格式，
            // 不要修改，否则算法库无法识别
            string priKeyContent = @"-----BEGIN PRIVATE KEY-----
MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgkTfHxPa8YusG+va8
1CRztNQBOEr90TBEjlQBZ5d1Y0ChRANCAAS9isP/xLib7EZ1vS5OUy+gOsYBwees
PMDvWiTygPAUsGZv1PHLoa0ciqsElkO1fMGwNrzOKJx1Oo194Ri+SypV
-----END PRIVATE KEY-----";
            string pubKeyContent = @"-----BEGIN PUBLIC KEY-----
MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEvYrD/8S4m+xGdb0uTlMvoDrGAcHn
rDzA71ok8oDwFLBmb9Txy6GtHIqrBJZDtXzBsDa8ziicdTqNfeEYvksqVQ==
-----END PUBLIC KEY-----";
            TLSSigAPI api = new TLSSigAPI(1400000000, priKeyContent, pubKeyContent);
            Console.WriteLine(api.genSig("xiaojun"));
            Console.ReadKey();
        }
    }
}

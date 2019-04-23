## 重要说明
本项目前发现有兼容性问题，目前我们提供了另外解决方案，[这里查看](https://github.com/tencentyun/tls-sig-api-cs-cpp)。

## 说明
本项目提供了 tls sig api 的 C# 原生版。
TLSSigAPI.cs 为 api 的实现，TLSSigAPIDemo.cs 演示了如何使用。

## 源代码集成
将 TLSSigAPI.cs 下载开发者项目的目录下即可，api 实现仅依赖了 zlib.net 类库，请在项目中进行导入。

## 使用说明
### 使用默认过期时间
```C#
...
using tencentyun
...
// .net 导入私钥的接口与 openssl 不一致
// 这里私钥需要把下载到 pem 文件中的头尾去掉
string privKeyContent = 
    "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgkTfHxPa8YusG+va8"
    + "1CRztNQBOEr90TBEjlQBZ5d1Y0ChRANCAAS9isP/xLib7EZ1vS5OUy+gOsYBwees"
    + "PMDvWiTygPAUsGZv1PHLoa0ciqsElkO1fMGwNrzOKJx1Oo194Ri+SypV";
TLSSigAPI api = new TLSSigAPI(1400000000, privKeyContent, "");
Console.WriteLine(api.genSig("xiaojun"));
```

### 使用指定过期时间
```C#
...
using tencentyun
...
// .net 导入私钥的接口与 openssl 不一致
// 这里私钥需要把下载到 pem 文件中的头尾去掉
string privKeyContent = 
    "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgkTfHxPa8YusG+va8"
    + "1CRztNQBOEr90TBEjlQBZ5d1Y0ChRANCAAS9isP/xLib7EZ1vS5OUy+gOsYBwees"
    + "PMDvWiTygPAUsGZv1PHLoa0ciqsElkO1fMGwNrzOKJx1Oo194Ri+SypV";
TLSSigAPI api = new TLSSigAPI(1400000000, privKeyContent, "");
// 使用指定 30 天有效期
Console.WriteLine(api.genSig("xiaojun", 3600*24*30));
```

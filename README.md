## 说明
本项目提供了腾讯云云通信账号 tls sig api 的 C# 原生版本。

## 使用 VS 集成
在 Visual Studio 中按照 `工具`->`NuGet 包管理器`->`管理解决方案的 NuGet 程序包`，然后搜索 `tls-sig-api` 进行安装。

## 使用 NuGet 集成
项目已经打包上传至 nuget.org 包管理仓库，可以使用 nuget 直接安装
```
PM> Install-Package tls-sig-api
```
多种命令行安装方式[这里](https://www.nuget.org/packages/tls-sig-api)可以查看。



## 使用源代码集成
将 `tls-sig-api-cs/TLSSigAPI.cs` 下载放置到需要的目录，按照 demo 中的示例代码进行调用即可。其中代码依赖了下列开发库，请进行手动安装，
```
Portable.BouncyCastle
zlib.net-mutliplatform
```

## 使用说明
### 使用默认过期时间
```C#
...
using tencentyun
...
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
```

### 使用指定过期时间
```C#
...
using tencentyun
...
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
Console.WriteLine(api.genSig("xiaojun", 3600*24*30));
```

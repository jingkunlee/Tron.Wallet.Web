# A website wallet for tron
波场本地网页版钱包

功能：
1、导入私钥、助记词；
2、随机生成私钥；
3、转出 TRX、USDT；
4、一键设置多签；

### Preview / 页面预览
![](https://img2023.cnblogs.com/blog/3167729/202304/3167729-20230415124607478-294075283.png)

部署在本地 iis， 然后在 hosts 绑定一个自定义域名，然后就可以愉快的玩耍了
```
127.0.0.1    tron.wallet.com
```

部署说明：
VS 2022 可以直接 F5 启动运行；
如果需要部署 IIS ，需要确保服务器安装 [【ASP.NET Core Runtime 6.0.25 Host Binding】](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.25-windows-hosting-bundle-installer)
通过 VS　发布，目录需要添加　IIS_USER 和 Everyone 权限，应用程序池设置【无代码托管】；
可以搜索 IIS  如何部署  .NET　CORE 教程；

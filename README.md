<h1 align="center">CosmosFTP Server 🚀</h1>
<p>
  <a href="https://www.nuget.org/packages/CosmosFtpServer/" target="_blank">
    <img alt="Version" src="https://img.shields.io/nuget/v/CosmosFtpServer.svg" />
  </a>
  <a href="https://github.com/CosmosOS/CosmosFtp/blob/main/LICENSE.txt" target="_blank">
    <img alt="License: BSD Clause 3 License" src="https://img.shields.io/badge/license-BSD License-yellow.svg" />
  </a>
</p>

> CosmosFTP is a FTP server made in C# for the Cosmos operating system construction kit.

## Usage

Install the Nuget Package from [Nuget](https://www.nuget.org/packages/CosmosFtpServer/) or [Github](https://github.com/CosmosOS/CosmosFtp/packages/1467237):

```PM
Install-Package CosmosFtpServer -Version 1.0.6
```

```PM
dotnet add PROJECT package CosmosFtpServer --version 1.0.6
```

Or add these lines to your Cosmos kernel .csproj:

```
<ItemGroup>
    <PackageReference Include="CosmosFtpServer" Version="1.0.6" NoWarn="NU1604;NU1605" />
</ItemGroup>
```

You can find more information about the FTP server and how to connect from a remote computer in the [Cosmos Documentation](https://cosmosos.github.io/articles/Kernel/Network.html#ftp).

##### note: Only ACTIVE transfer mode is currently supported due to problems with the Cosmos TCP/IP stack.
##### Port of a C written Epitech project: [NWP_myftp_2019](https://github.com/valentinbreiz/NWP_myftp_2019)

## Authors

👤 **[@valentinbreiz](https://github.com/valentinbreiz)**

## 🤝 Contributing

Contributions, issues and feature requests are welcome!<br />Feel free to check [issues page](https://github.com/CosmosOS/CosmosFtp/issues). 

## Show your support

Give a ⭐️ if this project helped you!

## 📝 License

Copyright © 2022 [CosmosOS](https://github.com/CosmosOS).<br />
This project is [BSD Clause 3](https://github.com/CosmosOS/CosmosFtp/blob/main/LICENSE.txt) licensed.

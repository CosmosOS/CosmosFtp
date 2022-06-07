﻿<h1 align="center">CosmosFTP Server 🚀</h1>
<p>
  <img alt="Version" src="https://img.shields.io/nuget/v/CosmosFtpServer.svg" />
  <a href="https://github.com/CosmosOS/CosmosFtp/blob/main/LICENSE.txt" target="_blank">
    <img alt="License: BSD Clause 3 License" src="https://img.shields.io/badge/license-BSD License-yellow.svg" />
  </a>
</p>

> CosmosFTP is a FTP server made in C# for the Cosmos operating system construction kit.

## Usage

Install the Nuget Package from [Nuget](https://www.nuget.org/packages/CosmosFtpServer/) or [Github](https://github.com/CosmosOS/CosmosFtp/packages/1467237):

```PM
Install-Package CosmosFtpServer -Version 1.0.1
```

```PM
dotnet add PROJECT package CosmosFtpServer --version 1.0.1
```

Or add these lines to your Cosmos kernel .csproj:

```
<ItemGroup>
    <PackageReference Include="CosmosFtpServer" Version="1.0.1" NoWarn="NU1604;NU1605" />
</ItemGroup>
```

##### note: Only ACTIVE transfer mode is currently supported due to problems with the Cosmos TCP/IP stack.

## Authors

👤 **[@valentinbreiz](https://github.com/valentinbreiz)**

## 🤝 Contributing

Contributions, issues and feature requests are welcome!<br />Feel free to check [issues page](https://github.com/CosmosOS/CosmosFtp/issues). 

## Show your support

Give a ⭐️ if this project helped you!

## 📝 License

Copyright © 2022 [CosmosOS](https://github.com/CosmosOS).<br />
This project is [BSD Clause 3](https://github.com/CosmosOS/CosmosFtp/blob/main/LICENSE.txt) licensed.

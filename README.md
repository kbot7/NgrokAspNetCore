# Credits
- Original author by kg73: https://github.com/kg73/NgrokAspNetCore
- Fork that enables .NET Core 3 and Linux support by doug62: https://github.com/doug62/NgrokAspNetCore/tree/linux-core3

# FluffySpoon.AspNet.NGrok
Extensions to start Ngrok automatically from the AspNetCore pipeline. Useful to enable for local development when a public URL is needed.

## Setting it up

Add `AddNgrok` to your service registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNGrok();
}

public void Configure(IApplicationBuilder app)
{
    app.UseNGrok();

    //the rest of the existing code goes here.
}
```

When the application starts up, NGrok will launch automatically and create a tunnel to the application. 

Note: If NGrok is not installed, it will be downloaded automatically to the execution directory.

## Getting the exposed URL
Simple inject a `INGrokHostedService` and call its `GetTunnelsAsync` method.

`INGrokHostedService` also has a `Ready` event that you can listen to, if you'd rather like that.

## Configuration
NGrok can be configured when registering it in the services collection by passing in an `NGrokOptions` instance in the `AddNGrok` method

#### NGrokOptions
| Option | Description |
| --- | --- |
| NgrokPath | Path to ngrok.exe. If not provided, it will default to the current directory, and search the Windows PATH variable. If all attempts fail, it will attempt to download it from the Ngrok CDN to the executing directory |
| ApplicationHttpUrl | The local URL to proxy to. If not provided, it will default to the first HTTP URL registered. It should work fine automatically when hosted from kestrel. Haven't tested in IIS. |
| NgrokConfigProfile | The name of the config profile specified in an ngrok.yml config file. See here for details on the ngrok.yml format: https://ngrok.com/docs#config. This will override all other settings |

## Contributing
Feedback and Contributions are greatly welcome. 

Please submit any bugs, issues, questions, or feature requests, by [submitting an issue](https://github.com/dprothero/NgrokExtensions/issues)

To submit pull requests, fork this repository to your own account, then submit a pull request back to this repository.

## Limitations
* Currently only supports tunnels to HTTP
* Support for ngrok.yml configuration is limited. Pull requests are welcome to improve this area

## Future Enhacements
* Support for a standard appsettings configuration format
* Better support for ngrok.yml
* Support for TCP, TLS
* Support for sub-domains

## Change Log
* v0.9.0 Initial release



* * *




## Copyright
Licensed under the MIT license. See the LICENSE file in the project root for more information.
Copyright (c) 2019 Kevin Gysberg, David Prothero
Some code forked from https://github.com/dprothero/NgrokExtensions and modified.
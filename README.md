# Ngrok for AspNetCore
Extensions to start Ngrok automatically from the AspNetCore pipeline. Useful to enable for local development when a public URL is needed.

## How To

Add `AddNgrok` to your service registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNgrok();
    services.AddMvc()
}
```


Start Ngrok in the WebHost pipeline
```csharp
public static async Task Main(string[] args)
{
    var builder = CreateWebHostBuilder(args);

    var host = builder.Build();

    // Start Ngrok here
    await host.StartNgrokAsync();

    host.Run();
}

public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    	.UseStartup<Startup>();
```

When the application starts up, ngrok will launch automatically and create a tunnel to the applicaiton. 

Note: If ngrok is not installed, it will be downloaded automatically to the execution directory

## Configuration
Ngrok can be configured when registering it in the services collection by passing in an `NgrokOptions` instance in the `AddNgrok` method

#### NgrokOptions
| Option | Description |
| --- | --- |
| NgrokPath | Path to ngrok.exe. If not provided, it will default to the current directory, and search the Windows PATH variable. If all attempts fail, it will attempt to download it from the Ngrok CDN to the executing directory |
| ApplicationHttpUrl | The local Url to proxy to. If not provided, it will default to the first http url registered. It should work fine automatically when hosted from kestrel. Haven't tested in IIS. |
| NgrokConfigProfile | The name of the config profile specified in an ngrok.yml config file. See here for details on the ngrok.yml format: https://ngrok.com/docs#config. This will override all other settings |

## Contributing
Feedback and Contributions are greatly welcome. 

Please submit any bugs, issues, questions, or feature requests, by [submitting an issue](https://github.com/dprothero/NgrokExtensions/issues)

To submit pull requests, fork this repository to your own account, then submit a pull request back to this repository.

## Limitations
* Currently only supports tunnels to http
* Support for ngrok.yml configuration is limited. Pull requests are welcome to improve this area

## Future Enhacements
* Support for a standard appsettings configuration format
* Better support for ngrok.yml
* Support for tcp, tls
* Support for subdomains

## Change Log
* v0.9.0 Initial release



* * *




## Copyright
Licensed under the MIT license. See the LICENSE file in the project root for more information.
Copyright (c) 2019 Kevin Gysberg, David Prothero
Some code forked from https://github.com/dprothero/NgrokExtensions and modified.
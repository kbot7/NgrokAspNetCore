# Credits
- Original project by kg73: https://github.com/kg73/NgrokAspNetCore
- Fork that enables .NET Core 3 and Linux support by doug62 - which this project is based on: https://github.com/doug62/NgrokAspNetCore/tree/linux-core3
- Original project for Visual Studio by dprothero: https://github.com/dprothero/NgrokExtensions

# FluffySpoon.AspNet.NGrok
Extensions to start Ngrok automatically from the AspNetCore pipeline. Useful to enable for local development when a public URL is needed.

## Setting it up

Add `AddNGrok` to your service registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNGrok(/* optional options here */);
}
```

You also need to call `UseNGrok` in your `Program` class when making the builder.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(x => x
            .UseStartup<Startup>()
            .UseNGrok());
```

When the application starts up, NGrok will launch automatically and create a tunnel to the application. 

If NGrok is not installed, it will be downloaded automatically to the execution directory.

### Inferring the application URL
If you don't specify a `ApplicationHttpUrl` property in the options when calling `AddNGrok`, you have to insert the following call in your `Startup` class' `Configure` method:

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseNGrokAutomaticUrlDetection();
}
```

## Getting the exposed URL
Simply inject an `INGrokHostedService` and call its `GetTunnelsAsync` method.

`INGrokHostedService` also has a `Ready` event that you can listen to, if you'd rather like that.

## Configuration
NGrok can be configured when registering it in the services collection by passing in an `NGrokOptions` instance in the `AddNGrok` method

#### NGrokOptions
| Option | Description |
| --- | --- |
| ApplicationHttpUrl | The local URL to proxy to. If not provided, it will default to the first HTTP URL registered. It should work fine automatically when hosted from kestrel. Haven't tested in IIS. |
| ShowNGrokWindow | Whether the NGrok window will be shown. Useful for debugging purposes. |

## Contributing
Feedback and Contributions are greatly welcome. 

Please submit any bugs, issues, questions, or feature requests, by submitting an issue.

To submit pull requests, fork this repository to your own account, then submit a pull request back to this repository.

## Limitations
* Currently only supports tunnels to HTTP
* Support for ngrok.yml configuration is limited. Pull requests are welcome to improve this area

## Copyright
Licensed under the MIT license. See the LICENSE file in the project root for more information.
Copyright (c) 2019 Kevin Gysberg, David Prothero

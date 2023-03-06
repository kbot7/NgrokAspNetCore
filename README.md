# WARNING
This project has been archived and out-of-date.
ASP.NET Core Dev Tunnels in Visual Studio 2022 provides similar functionality.
https://learn.microsoft.com/en-us/aspnet/core/test/dev-tunnels?view=aspnetcore-7.0

# Ngrok for Asp.Net Core
Ngrok.AspNetCore is a set of extensions to start Ngrok automatically from the AspNetCore pipeline. Useful to enable for local development when a public URL is needed. By default, an ngrok process will be started on application startup. Then an ngrok tunnel will be established to the local application URL.

## Setting it up

Add `AddNgrok` to your service registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNgrok();
}
```

When the application starts up, Ngrok will launch automatically and create a tunnel to the application by default. 

If Ngrok is not installed, it will be downloaded automatically to the execution directory by default.

## Getting the Ngrok URL
Simply inject an `INgrokHostedService` and call its `GetTunnelsAsync` method.

`INgrokHostedService` also has a `Ready` event that you can listen to, if you'd rather like that.

## Configuration
Ngrok can be configured when registering it in the services collection by passing in an `Action<NgrokOptions>` in the `AddNgrok` method
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNgrok(options =>
    {
        options.Disable = false;
        options.ManageNgrokProcess = true;
        options.DetectUrl = true;
    });
}
```


#### NgrokOptions
| Option | Default | Description |
| --- | --- | --- |
| Disable | `false` | Disable all Ngrok integration features. |
| DetectUrl | `true` | Detect http url via `Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature`. |
| ApplicationHttpUrl | `null` | Sets the local URL Ngrok will proxy to. Must be http (not https) at this time. Only used when `DetectUrl` is false. |
| ManageNgrokProcess | `true` | Launch and manage an ngrok process if one is not already running on startup. |
| NgrokPath | `null` | Path to the Ngrok executable. If not set, the execution directory and PATH will be searched.  |
| DownloadNgrok | `true` | Download Ngrok if not found in local directory or PATH. Requires `ManageNgrokProcess`.  |
| ProcessStartTimeoutMs | `5000` | Time in milliseconds to wait for the ngrok process to start. Requires `ManageNgrokProcess`.  |
| RedirectLogs | `true` | Redirect Ngrok process logs to Microsoft.Extensions.Logging.  |
| NgrokConfigProfile | `null` | Not currently in use. |

## Limitations
* Currently only supports tunnels to HTTP
* Support for ngrok.yml configuration is limited. Pull requests are welcome to improve this area

## Contributing
Feedback and Contributions are greatly welcome. 

Please submit any bugs, issues, questions, or feature requests, by submitting an issue.

To submit pull requests, fork this repository to your own account, then submit a pull request back to this repository.

## Credits
- doug62 - enabled .NET Core 3 and Linux support 
- ffMathy - improved testing and linux support, and various other fixes
- Original project for Visual Studio by dprothero: https://github.com/dprothero/NgrokExtensions

## Copyright
Licensed under the MIT license. See the LICENSE file in the project root for more information.
Copyright (c) 2019 Kevin Gysberg, David Prothero

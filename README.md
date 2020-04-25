# Credits
- doug62 - enabled .NET Core 3 and Linux support 
- ffMathy - improved testing and linux support, and various other fixes
- Original project for Visual Studio by dprothero: https://github.com/dprothero/NgrokExtensions

# Ngrok for Asp.Net Core
Extensions to start Ngrok automatically from the AspNetCore pipeline. Useful to enable for local development when a public URL is needed.

## Setting it up

Add `AddNgrok` to your service registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNgrok();
}
```

When the application starts up, Ngrok will launch automatically and create a tunnel to the application. 

If Ngrok is not installed, it will be downloaded automatically to the execution directory.

## Getting the exposed URL
Simply inject an `INgrokHostedService` and call its `GetTunnelsAsync` method.

`INgrokHostedService` also has a `Ready` event that you can listen to, if you'd rather like that.

## Configuration
Ngrok can be configured when registering it in the services collection by passing in an `NgrokOptions` instance in the `AddNgrok` method

#### NgrokOptions
| Option | Description |
| --- | --- |
| ApplicationHttpUrl | The local URL to proxy to. If not provided, it will default to the first HTTP URL registered. It should work fine automatically when hosted from kestrel. Haven't tested in IIS. |
| ShowNgrokWindow | Whether the Ngrok window will be shown. Useful for debugging purposes. |

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

// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NgrokAspNetCore.Exceptions;
using NgrokExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NgrokAspNetCore
{
	public static class NgrokAspNetCoreExtensions
	{
		public static void AddNgrok(this IServiceCollection services, NgrokOptions options = null)
		{
			services.TryAddSingleton<NgrokProcess>();

			services.AddHttpClient<NgrokDownloader>();
			services.AddHttpClient<NgrokLocalApiClient>();

			services.TryAddSingleton<NgrokOptions>(options ?? new NgrokOptions());

			services.AddLogging();
		}

		/// <summary>
		/// Start ngrok.exe with configured options
		/// </summary>
		/// <exception cref="NgrokStartFailedException">Throws when ngrok failed to start</exception>
		/// <exception cref="NgrokUnsupportedException">Throws when ngrok is not supported on the OS and architecture</exception>
		/// <exception cref="NgrokNotFoundException">Throws when ngrok is not found and is unable to be downloaded automatically</exception>
		/// /// <exception cref="NgrokHostNotFoundException">Throws when a local address to tunnel to is not found and not configured</exception>
		/// <param name="host"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<Tunnel>> StartNgrokAsync(this IWebHost host)
		{
			var services = host.Services;

			// Configure NgrokOptions from services and IWebHost.Features
			var options = await ConfigureOptions(host);

			// Start Ngrok
			var ngrokClient = services.GetRequiredService<NgrokLocalApiClient>();

			return await ngrokClient.StartTunnelsAsync(options.NgrokPath);
		}

		private static async Task<NgrokOptions> ConfigureOptions(IWebHost host)
		{
			var services = host.Services;
			var options = services.GetRequiredService<NgrokOptions>();

			// Ensure ngrok is installed and set path in options. Throws if not found and not downloaded
			var ngrokDownloader = services.GetRequiredService<NgrokDownloader>();
			var ngrokFullPath = await ngrokDownloader.EnsureNgrokInstalled(options);
			options.NgrokPath = ngrokFullPath;

			// If config is driven off an NgrokConfig, short circuit
			if (options.NgrokConfigProfile.HasValue())
			{
				return options;
			}

			// Check if an app url was provided and if it is valid. If so, short circuit
			var optionsUrlValid = options.ApplicationHttpUrl.HasValue() && Uri.TryCreate(options.ApplicationHttpUrl, UriKind.Absolute, out Uri uri);
			if (optionsUrlValid)
			{
				return options;
			}

			// Since the app address wasn't specified anywhere else, grab it from the server features. If null, throw exception
			var addresses = (host.ServerFeatures[typeof(IServerAddressesFeature)] as IServerAddressesFeature)?.Addresses;
			addresses = addresses ?? throw new NgrokHostNotFoundException();

			// Get first http address from address list. Throw exception if none found
			// Note: Make this configurable if we eventually integrate TLS termination with Core HTTPS/HSTS
			var localAddress = addresses.FirstOrDefault(a => a.StartsWith("http://"));
			if (!localAddress.HasValue())
			{
				throw new NgrokHostNotFoundException();
			}
			options.ApplicationHttpUrl = localAddress;

			return options;
		}
	}
}

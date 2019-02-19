// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
		/// <param name="host"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<Tunnel>> StartNgrokAsync(this IWebHost host)
		{
			var services = host.Services;

			// Configure NgrokOptions from services and IWebHost.Features
			var options = await ConfigureOptions(host);

			// Start Ngrok
			var ngrokClient = services.GetRequiredService<NgrokLocalApiClient>();

			var tunnels = await ngrokClient.StartTunnelsAsync(options.NgrokPath);

			return tunnels;
		}

		private static async Task<NgrokOptions> ConfigureOptions(IWebHost host)
		{
			var services = host.Services;
			var options = services.GetRequiredService<NgrokOptions>();

			// Set address automatically if not provided or invalid
			var addresses = (host.ServerFeatures[typeof(IServerAddressesFeature)] as IServerAddressesFeature)?.Addresses;
			if (!string.IsNullOrWhiteSpace(options.ApplicationHttpUrl) || !Uri.TryCreate(options.ApplicationHttpUrl, UriKind.Absolute, out Uri uri))
			{
				options.ApplicationHttpUrl = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault();
			}

			// Ensure ngrok is installed and set path
			var ngrokDownloader = services.GetRequiredService<NgrokDownloader>();
			var ngrokFullPath = await ngrokDownloader.EnsureNgrokInstalled(options);
			options.NgrokPath = ngrokFullPath;

			return options;
		}
	}
}

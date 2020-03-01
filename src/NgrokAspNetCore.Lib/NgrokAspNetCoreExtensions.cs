// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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


		public static async Task<IEnumerable<Tunnel>> StartNgrokAsyncWorker(IServiceProvider services, NgrokOptions options)
		{
			// Start Ngrok
			var ngrokClient = services.GetRequiredService<NgrokLocalApiClient>();
			return await ngrokClient.StartTunnelsAsync(options.NgrokPath);
		}

		public static async Task<IEnumerable<Tunnel>> StartNgrokAsync(this IHost host)
		{
			// Configure NgrokOptions from services and IWebHost.Features
			var options = await ConfigureOptions(host);
			return await StartNgrokAsyncWorker(host.Services, options);
		}

		public static async Task<IEnumerable<Tunnel>> StartNgrokAsync(this IWebHost host)
		{
			// Configure NgrokOptions from services and IWebHost.Features
			var options = await ConfigureOptions(host);
			return await StartNgrokAsyncWorker(host.Services, options);
		}



		private static async Task<NgrokOptions> ConfigureOptionsWorker(IServiceProvider services)
		{
			var options = services.GetRequiredService<NgrokOptions>();

			// Set address automatically if not provided or invalid
			var addresses = services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.ToArray();
			if (string.IsNullOrWhiteSpace(options.ApplicationHttpUrl) || !Uri.TryCreate(options.ApplicationHttpUrl, UriKind.Absolute, out Uri uri))
			{
				options.ApplicationHttpUrl = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault();
			}

			// Ensure ngrok is installed and set path
			var ngrokDownloader = services.GetRequiredService<NgrokDownloader>();
			var ngrokFullPath = await ngrokDownloader.EnsureNgrokInstalled(options);
			options.NgrokPath = ngrokFullPath;

			return options;
		}

		private static async Task<NgrokOptions> ConfigureOptions(IHost host)
		{
			return await ConfigureOptionsWorker(host.Services);
		}

		private static async Task<NgrokOptions> ConfigureOptions(IWebHost host)
		{
			return await ConfigureOptionsWorker(host.Services);
		}
	}
}

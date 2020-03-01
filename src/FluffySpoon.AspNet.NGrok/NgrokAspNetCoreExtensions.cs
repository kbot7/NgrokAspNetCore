// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.NgrokModels;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
	public static class NgrokAspNetCoreExtensions
	{
		public static void AddNgrok(this IServiceCollection services, NgrokOptions options = null)
		{
			services.TryAddSingleton<NgrokProcess>();

			services.AddHttpClient<NgrokDownloader>();
			services.AddHttpClient<NgrokLocalApiClient>();

			services.TryAddSingleton(options ?? new NgrokOptions());

			services.AddLogging();
		}


		public static async Task<IEnumerable<Tunnel>> StartNgrokAsyncWorker(IServiceProvider services, NgrokOptions options)
		{
			// Start Ngrok
			var ngrokClient = services.GetRequiredService<NgrokLocalApiClient>();
			return await ngrokClient.StartTunnelsAsync(options.NgrokPath);
		}

		public static async Task<IEnumerable<Tunnel>> StartNgrokAsync(this IWebHost host)
		{
			var options = await ConfigureOptions(host);
			return await StartNgrokAsyncWorker(host.Services, options);
		}

		private static async Task<NgrokOptions> ConfigureOptions(IWebHost host)
        {
            var services = host.Services;
            var options = services.GetRequiredService<NgrokOptions>();

            // Set address automatically if not provided or invalid
            var addresses = host.ServerFeatures
                .Get<IServerAddressesFeature>()
                .Addresses;
            if (string.IsNullOrWhiteSpace(options.ApplicationHttpUrl) || !Uri.TryCreate(options.ApplicationHttpUrl, UriKind.Absolute, out _))
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

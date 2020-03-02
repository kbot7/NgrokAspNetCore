// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.NGrokModels;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
	public static class NGrokAspNetCoreExtensions
	{
		public static void AddNGrok(this IServiceCollection services, NGrokOptions options = null)
		{
			services.TryAddSingleton<NGrokProcess>();

			services.AddHttpClient<NGrokDownloader>();
			services.AddHttpClient<NGrokLocalApiClient>();

			services.TryAddSingleton(options ?? new NGrokOptions());

			services.AddLogging();

            services.AddSingleton<NGrokHostedService>();
            services.AddSingleton<INGrokHostedService>(p => p.GetRequiredService<NGrokHostedService>());
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<NGrokHostedService>());
        }

        public static void UseNGrok(this IApplicationBuilder builder)
        {
            var ngrokService = builder.ApplicationServices.GetRequiredService<NGrokHostedService>();
			ngrokService.InjectServerAddressesFeature(builder.ServerFeatures.Get<IServerAddressesFeature>());
        }
	}
}

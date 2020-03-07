// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluffySpoon.AspNet.NGrok
{
	public static class NGrokAspNetCoreExtensions
	{
        public static IApplicationBuilder UseNGrokAutomaticUrlDetection(this IApplicationBuilder app)
        {
            var service = app.ApplicationServices.GetRequiredService<NGrokHostedService>();
            service.InjectServerAddressesFeature(app.ServerFeatures.Get<IServerAddressesFeature>());

            return app;
        }

        public static IServiceCollection AddNGrok(this IServiceCollection services, NGrokOptions? options = null)
        {
            services.TryAddSingleton<NGrokProcess>();

            services.AddHttpClient<NGrokDownloader>();
            services.AddHttpClient<NGrokApiClient>();

            services.AddLogging();

            services.AddSingleton<NGrokHostedService>();
            services.AddSingleton<INGrokHostedService>(p => p.GetRequiredService<NGrokHostedService>());

            services.AddSingleton(options ?? new NGrokOptions());

            return services;
        }

        public static IWebHostBuilder UseNGrok(this IWebHostBuilder builder)
        {
            return builder
                .ConfigureServices((context, services) =>
                {
                    if (services.All(x => x.ImplementationType != typeof(NGrokHostedService)))
                        return;

                    services.AddSingleton<IHostedService>(p => p.GetRequiredService<NGrokHostedService>());
                });
        }
	}
}

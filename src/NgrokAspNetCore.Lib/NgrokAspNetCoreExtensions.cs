// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NgrokAspNetCore.Lib.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ngrok.ApiClient;

namespace NgrokAspNetCore.Lib
{
	public static class NgrokAspNetCoreExtensions
	{
		public static void AddNgrok(this IServiceCollection services, NgrokOptions options = null)
		{
			services.TryAddSingleton<NgrokProcess>();

			services.AddHttpClient<NgrokDownloader>();
			services.AddHttpClient<NgrokHttpClient>();

			services.TryAddSingleton(options ?? new NgrokOptions());

			services.AddLogging();

            services.AddHostedService<NgrokHostedService>();
			services.AddSingleton<NgrokProcessMgr>();
            services.AddSingleton<INgrokHostedService>(p => p.GetRequiredService<NgrokHostedService>());
        }
	}
}

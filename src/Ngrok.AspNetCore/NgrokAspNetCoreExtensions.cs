// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ngrok.ApiClient;
using Ngrok.AspNetCore.Services;
using System;

namespace Ngrok.AspNetCore
{
	public static class NgrokAspNetCoreExtensions
	{
		public static IServiceCollection AddNgrok(this IServiceCollection services, Action<NgrokOptions> action = null)
		{
			var optBuilder = ConfigureOptionsBuilder(services, action);

			services.TryAddSingleton<NgrokProcessMgr>();
			services.AddHttpClient<NgrokDownloader>();
			services.AddHttpClient<INgrokApiClient, NgrokHttpClient>();

			services.AddLogging();

			services.AddSingleton<NgrokHostedService>();
			services.AddSingleton<INgrokHostedService>(p => p.GetRequiredService<NgrokHostedService>());
			services.AddSingleton<IHostedService>(p => p.GetRequiredService<NgrokHostedService>());

			return services;
		}

		private static OptionsBuilder<NgrokOptions> ConfigureOptionsBuilder(IServiceCollection services, Action<NgrokOptions> action = null)
		{
			var optBuilder = services.AddOptions<NgrokOptions>();
			if (action != null)
			{
				optBuilder.Configure(action);
			}
			optBuilder.PostConfigure(PostConfigure);
			optBuilder.Validate(ValidateUrlDetectOpt, "Must supply an ApplicationHttpUrl if DetectUrl is false");
			return optBuilder;
		}

		private static void PostConfigure(NgrokOptions opt)
		{
			if (opt.ManageNgrokProcess == false)
			{
				opt.DownloadNgrok = false;
				opt.RedirectLogs = false;
				opt.NgrokConfigProfile = null;
				opt.ProcessStartTimeoutMs = 0;
			}
		}

		private static bool ValidateUrlDetectOpt(NgrokOptions opt)
		{
			if (opt.DetectUrl == false && string.IsNullOrWhiteSpace(opt.ApplicationHttpUrl))
			{
				return false;
			}
			return true;
		}
	}
}
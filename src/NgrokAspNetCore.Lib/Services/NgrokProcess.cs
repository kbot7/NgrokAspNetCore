// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero, Kevin Gysberg
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NgrokExtensions

using Microsoft.Extensions.Hosting;
using NgrokAspNetCore.Internal;
using System.Collections.Generic;
using System.Diagnostics;

namespace NgrokAspNetCore
{
	public class NgrokProcess
	{
		private Process _process;
		private readonly NgrokOptions _options;

		public NgrokProcess(IApplicationLifetime applicationLifetime, NgrokOptions options)
		{
			applicationLifetime.ApplicationStopping.Register(Stop);
			_options = options;
		}

		public void StartNgrokProcess(string ngrokPath)
		{
			var args = new List<string>();
			args.Add("start");

			if (_options.NgrokYmlConfigProfile.HasValue() && _options.ValidateNgrokYmlSettings())
			{
				// Set the config if requested and exists
				if (_options.NgrokYmlConfigPath.HasValue() && _options.NgrokYmlConfigPath.FileExists())
				{
					args.Add($"-config={_options.NgrokYmlConfigPath}");
				}

				// Set the profile name
				args.Add(_options.NgrokYmlConfigProfile);
			}
			else
			{
				// Not starting from config, so start none, and we will add the tunnels later from our internal config
				args.Add("--none");
			}

			var pi = new ProcessStartInfo(ngrokPath, string.Join(" ", args))
			{
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = true
			};


			Start(pi);
		}

		protected virtual void Start(ProcessStartInfo pi)
		{
			var startedProcess = Process.Start(pi);
			_process = startedProcess;
		}

		public void Stop()
		{
			if (_process != null && !_process.HasExited)
			{
				_process.Kill();
				foreach (var p in Process.GetProcessesByName("ngrok"))
				{
					p.Kill();
				}
			}
		}
	}
}

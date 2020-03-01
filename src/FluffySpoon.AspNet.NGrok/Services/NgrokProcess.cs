// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NGrokExtensions

using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok.Services
{
	public class NGrokProcess
	{
		private Process _process;

		public NGrokProcess(IApplicationLifetime applicationLifetime)
		{
			applicationLifetime.ApplicationStopping.Register(() => Stop());
		}

		public void StartNGrokProcess(string nGrokPath)
		{
			var pi = new ProcessStartInfo(nGrokPath, "start --none")
			{
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Normal
			};

			pi.UseShellExecute = true;

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
				foreach (var p in Process.GetProcessesByName("NGrok"))
				{
					p.Kill();
				}
			}
		}
	}
}

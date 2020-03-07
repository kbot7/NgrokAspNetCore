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
        private readonly NGrokOptions _ngrokOptions;
        private Process _process;

		public NGrokProcess(
            IApplicationLifetime applicationLifetime,
            NGrokOptions ngrokOptions)
        {
            _ngrokOptions = ngrokOptions;
            applicationLifetime.ApplicationStopping.Register(Stop);
        }

		public void StartNGrokProcess(string nGrokPath)
		{
            var processInformation = new ProcessStartInfo(nGrokPath, "start --none")
            {
                CreateNoWindow = !_ngrokOptions.ShowNGrokWindow, 
                WindowStyle = _ngrokOptions.ShowNGrokWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };

            Start(processInformation);
		}

		protected virtual void Start(ProcessStartInfo pi)
        {
            KillExistingNGrokProcesses();
            _process = Process.Start(pi);
		}

		public void Stop()
		{
            if (_process == null || _process.HasExited) 
                return;

            _process.Kill();
            KillExistingNGrokProcesses();
        }

        private static void KillExistingNGrokProcesses()
        {
            foreach (var p in Process.GetProcessesByName("ngrok"))
            {
                p.Kill();
            }
        }
    }
}

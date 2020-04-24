﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero, Kevin Gysberg
// Originally pulled from Github on 2019-01-13 at https://github.com/dprothero/NGrokExtensions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluffySpoon.AspNet.NGrok.Extensions;
using Mono.Unix;
using System.Runtime.InteropServices;

namespace FluffySpoon.AspNet.NGrok.Services
{
	public class NGrokProcess
	{
		private Process _process;
		private ILogger _ngrokProcessLogger;
		private readonly NGrokOptions _ngrokOptions;

		public Action ProcessStarted { get; set; }

		public NGrokProcess(
			IApplicationLifetime applicationLifetime,
			NGrokOptions ngrokOptions,
			ILoggerFactory loggerFactory
			)
		{
			_ngrokOptions = ngrokOptions;
			applicationLifetime.ApplicationStopping.Register(Stop);
			_ngrokProcessLogger = loggerFactory.CreateLogger("NGrokProcess");
		}

		public void StartNGrokProcess()
		{
			var processWindowStyle = _ngrokOptions.ShowNGrokWindow ?
				ProcessWindowStyle.Normal :
				ProcessWindowStyle.Hidden;

			var linuxProcessStartInfo = new ProcessStartInfo("/bin/bash", "-c \"" + Directory.GetCurrentDirectory() + "/ngrok start --none --log=stdout\"")
			{
				CreateNoWindow = true,
				WindowStyle = processWindowStyle,
				UseShellExecute = false,
				WorkingDirectory = Environment.CurrentDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var windowsProcessStartInfo = new ProcessStartInfo("NGrok.exe", "start --none --log=stdout")
			{
				CreateNoWindow = true,
				WindowStyle = processWindowStyle,
				UseShellExecute = false,
				WorkingDirectory = Environment.CurrentDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var processInformation = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
				windowsProcessStartInfo :
				linuxProcessStartInfo;

			Start(processInformation);
		}

		protected virtual void Start(ProcessStartInfo pi)
		{
			KillExistingNGrokProcesses();
			var process = new Process();
			process.StartInfo = pi;

			process.OutputDataReceived += ProcessStandardOutput;
			process.ErrorDataReceived += ProcessStandardError;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			_process = process;
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

		private void ProcessStandardError(object sender, DataReceivedEventArgs args)
		{
			if (!string.IsNullOrWhiteSpace(args.Data))
			{
				_ngrokProcessLogger.LogError(args.Data);
			}
		}

		private void ProcessStandardOutput(object sender, DataReceivedEventArgs args)
		{
			if (args == null || string.IsNullOrWhiteSpace(args.Data))
			{
				return;
			}

			// Fire event when NGrok Client Session is established
			const string clientSessionEstablishedKey = "obj=csess";
			if (args.Data.Contains(clientSessionEstablishedKey))
			{
				ProcessStarted?.Invoke();
			}

			// Build structured log data
			var data = NGrokLogExtensions.ParseLogData(args.Data);
			var logFormatData = data.Where(d => d.Key != "lvl" && d.Key != "t")
				.ToDictionary(e => e.Key, e => e.Value);
			var logFormatString = NGrokLogExtensions.GetLogFormatString(logFormatData);
			var logLevel = NGrokLogExtensions.ParseLogLevel(data["lvl"]);

			_ngrokProcessLogger.Log(logLevel, logFormatString, logFormatData.Values.ToArray());
		}
	}
}
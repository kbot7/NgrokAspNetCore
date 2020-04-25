using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NGrok.ApiClient;
using Ngrok.AspNetCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore.Services
{
	public class NGrokProcessMgr
	{
		private NGrokProcess _process;
		private readonly ILogger<NGrokProcessMgr> _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IApplicationLifetime _lifetime;

		private readonly NGrokOptions _options;
		private readonly INGrokApiClient _apiClient;

		private SemaphoreSlim _processStartSemaphore = new SemaphoreSlim(0, 1);

		public bool UsingManagedProcess { get; private set; }
		public bool IsStarted { get; private set; }

		public NGrokProcessMgr(
			ILogger<NGrokProcessMgr> logger,
			ILoggerFactory loggerFactory,
			IApplicationLifetime lifetime,
			NGrokOptions options,
			INGrokApiClient apiClient)
		{
			_logger = loggerFactory.CreateLogger<NGrokProcessMgr>();
			_loggerFactory = loggerFactory;
			_options = options;
			_apiClient = apiClient;
			_lifetime = lifetime;

		}

		public async Task EnsureNGrokStartedAsync()
		{
			// This allows an already-running NGrok instance to be used, instead of the one we are starting here. 
			if (await _apiClient.CheckIfLocalAPIUpAsync())
			{
				return;
			}

			try
			{
				UsingManagedProcess = true;
				_process = new NGrokProcess(_lifetime, _options, _loggerFactory);

				// Register OnProcessStarted Handler
				_process.ProcessStarted += OnProcessStarted;

				// Start Process
				_process.StartNGrokProcess();

				// Wait for Process to be started
				await _processStartSemaphore.WaitAsync(TimeSpan.FromSeconds(_options.NGrokProcessStartTimeoutMs));

				// Verify API is up
				var IsAPIUp = await _apiClient.CheckIfLocalAPIUpAsync();

				if (!IsAPIUp)
				{
					throw new NGrokStartFailedException();
				}
			}
			catch (Exception ex)
			{
				throw new NGrokStartFailedException(ex);
			}
		}

		public Task StopNGrokAsync()
		{
			_process.Stop();
			return Task.CompletedTask;
		}

		private void OnProcessStarted()
		{
			IsStarted = true;
			_processStartSemaphore.Release();
		}


	}
}
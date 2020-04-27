using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ngrok.ApiClient;
using Ngrok.AspNetCore.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore.Services
{
	public class NgrokProcessMgr
	{
		private NgrokProcess _process;
		private readonly ILogger<NgrokProcessMgr> _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IApplicationLifetime _lifetime;

		private readonly NgrokOptions _options;
		private readonly INgrokApiClient _apiClient;

		private SemaphoreSlim _processStartSemaphore = new SemaphoreSlim(0, 1);

		public bool UsingManagedProcess { get; private set; }
		public bool IsStarted { get; private set; }

		public NgrokProcessMgr(
			ILoggerFactory loggerFactory,
			IApplicationLifetime lifetime,
			IOptionsMonitor<NgrokOptions> optionsAccessor,
			INgrokApiClient apiClient)
		{
			_logger = loggerFactory.CreateLogger<NgrokProcessMgr>();
			_loggerFactory = loggerFactory;
			_options = optionsAccessor.CurrentValue;
			_apiClient = apiClient;
			_lifetime = lifetime;

		}

		public async Task EnsureNgrokStartedAsync(CancellationToken cancellationToken = default)
		{
			// This allows an already-running Ngrok instance to be used, instead of the one we are starting here. 
			if (await _apiClient.CheckIfLocalAPIUpAsync(cancellationToken))
			{
				return;
			}

			if (!_options.ManageNgrokProcess)
			{
				throw new NgrokStartFailedException("No running Ngrok process found and ManageNgrokProcess is disabled");
			}

			try
			{
				UsingManagedProcess = true;
				_process = new NgrokProcess(_lifetime, _loggerFactory, _options);

				// Register OnProcessStarted Handler
				_process.ProcessStarted += OnProcessStarted;

				// Start Process
				_process.StartNgrokProcess();

				// Wait for Process to be started
				await _processStartSemaphore.WaitAsync(TimeSpan.FromMilliseconds(_options.ProcessStartTimeoutMs), cancellationToken);

				// Verify API is up
				var IsAPIUp = await _apiClient.CheckIfLocalAPIUpAsync(cancellationToken);

				if (!IsAPIUp)
				{
					throw new NgrokStartFailedException();
				}
			}
			catch (Exception ex)
			{
				throw new NgrokStartFailedException(ex);
			}
		}

		public Task StopNgrokAsync()
		{
			_process?.Stop();
			return Task.CompletedTask;
		}

		private void OnProcessStarted()
		{
			IsStarted = true;
			_processStartSemaphore.Release();
		}


	}
}
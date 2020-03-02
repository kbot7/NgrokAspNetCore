using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ngrok.ApiClient;
using NgrokAspNetCore.Lib.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NgrokAspNetCore.Lib.Services
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
            ILogger<NgrokProcessMgr> logger,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime, 
            NgrokOptions options, 
            INgrokApiClient apiClient)
		{
            _logger = loggerFactory.CreateLogger<NgrokProcessMgr>();
            _loggerFactory = loggerFactory;
			_options = options;
            _apiClient = apiClient;
            _lifetime = lifetime;

        }

        private async Task StartNgrokAsync(string nGrokPath)
        {
            // This allows an already-running Ngrok instance to be used, instead of the one we are starting here. 
            if (await _apiClient.CheckIfLocalAPIUpAsync()) 
            {
                return;
            }

            try
            {
                UsingManagedProcess = true;
                _process = new NgrokProcess(_lifetime, _loggerFactory);
                
                // Register OnProcessStarted Handler
                _process.ProcessStarted += OnProcessStarted;

                // Start Process
                _process.StartNgrokProcess(nGrokPath);

                // Wait for Process to be started
                await _processStartSemaphore.WaitAsync(TimeSpan.FromSeconds(_options.NgrokProcessStartTimeoutMs));

                // Verify API is up
                var IsAPIUp = await _apiClient.CheckIfLocalAPIUpAsync();

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

        private Task StopNgrokAsync()
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

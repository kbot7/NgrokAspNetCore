using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using NGrok.ApiClient;
using Tunnel = NGrok.ApiClient.Tunnel;
using Microsoft.Extensions.Logging;

namespace FluffySpoon.AspNet.NGrok
{
	class NGrokHostedService : INGrokHostedService
	{
		private readonly NGrokOptions _options;
		private readonly NGrokDownloader _nGrokDownloader;
		private readonly NGrokProcessMgr _processMgr;
		private readonly INGrokApiClient _client;
		private readonly IServer _server;
		private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger<NGrokHostedService> _logger;

        private readonly TaskCompletionSource<IReadOnlyCollection<Tunnel>> _tunnelTaskSource;
        private readonly TaskCompletionSource<IReadOnlyCollection<string>> _serverAddressesSource;

        public async Task<IReadOnlyCollection<Tunnel>> GetTunnelsAsync()
        {
            if (_options.Disable)
                return Array.Empty<Tunnel>();

            return await WaitForTaskWithTimeout(_tunnelTaskSource.Task, 300_000, "No tunnels were found within 5 minutes. Perhaps the server was taking too long to start?");
        }

		public event Action<IEnumerable<Tunnel>> Ready;

		public NGrokHostedService(
			NGrokOptions options,
			NGrokDownloader nGrokDownloader,
			IServer server,
			IApplicationLifetime applicationLifetime,
			NGrokProcessMgr processMgr,
			INGrokApiClient client)
		{
			_options = options;
			_nGrokDownloader = nGrokDownloader;
			_server = server;
			_applicationLifetime = applicationLifetime;
			_processMgr = processMgr;
			_client = client;

            _tunnelTaskSource = new TaskCompletionSource<IReadOnlyCollection<Tunnel>>();
            _serverAddressesSource = new TaskCompletionSource<IReadOnlyCollection<string>>();
        }

		internal void InjectServerAddressesFeature(IServerAddressesFeature? feature)
		{
			_logger.LogDebug("Inferred hosting URLs as {ServerAddresses}.", feature?.Addresses);
			_serverAddressesSource.SetResult(feature?.Addresses.ToArray());
		}

		private async Task RunAsync()
        {
            if (_options.Disable)
                return;

            await _nGrokDownloader.DownloadExecutableAsync();
            await _processMgr.EnsureNGrokStartedAsync();
            var url = await AdjustApplicationHttpUrlIfNeededAsync();
            _logger.LogInformation("Picked hosting URL {Url}.", url);

            var tunnels = await StartTunnelsAsync(url);
            _logger.LogInformation("Tunnels {Tunnels} have been started.", tunnels);

            OnTunnelsFetched(tunnels);
        }

		private void OnTunnelsFetched(IEnumerable<Tunnel> tunnels)
		{
			if (tunnels == null)
				throw new ArgumentNullException(nameof(tunnels), "Tunnels was not expected to be null here.");

            _tunnelTaskSource.SetResult(tunnels.ToArray());
            Ready?.Invoke(tunnels);
        }

		private async Task<Tunnel[]?> StartTunnelsAsync(string address)
		{
			if (string.IsNullOrEmpty(address))
			{
				address = "80";
			}
			else
			{
				if (!int.TryParse(address, out _))
				{
					var url = new Uri(address);
					if (url.Port != 80 && url.Port != 443)
					{
						address = $"{url.Host}:{url.Port}";
					}
					else
					{
						if (address.StartsWith("http://"))
						{
							address = address.Remove(address.IndexOf("http://"), "http://".Length);
						}
						if (address.StartsWith("https://"))
						{
							address = address.Remove(address.IndexOf("https://"), "https://".Length);
						}
					}
				}
			}

			// Start Tunnel
			var tunnel = await _client.StartTunnelAsync(new StartTunnelRequest()
			{
				Name = System.AppDomain.CurrentDomain.FriendlyName,
				Address = address,
				Protocol = "http",
				HostHeader = address
			});

			// Get Tunnels
			var tunnels = (await _client.ListTunnelsAsync())
				.Where(t => t.Name == System.AppDomain.CurrentDomain.FriendlyName ||
				t.Name == $"{System.AppDomain.CurrentDomain.FriendlyName} (http)")
				?.ToArray();

			return tunnels;
		}

        private async Task<T> WaitForTaskWithTimeout<T>(Task<T> task, int timeoutInMilliseconds, string timeoutMessage)
        {
            if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)) == task)
                return await task;

            throw new InvalidOperationException(timeoutMessage);
        }

        private async Task<string> AdjustApplicationHttpUrlIfNeededAsync()
        {
            var url = _options.ApplicationHttpUrl;

            if (string.IsNullOrWhiteSpace(url))
            {
                var addresses = await WaitForTaskWithTimeout(
                    _serverAddressesSource.Task,
                    30000,
                    $"No {nameof(NGrokOptions.ApplicationHttpUrl)} was set in the settings, and the URL of the server could not be inferred within 30 seconds. Perhaps you are missing a call to {nameof(NGrokAspNetCoreExtensions.UseNGrokAutomaticUrlDetection)} in your Configure method of your Startup class?");
                if (addresses != null)
                {
                    url = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault();
                    url = url?.Replace("*", "localhost", StringComparison.InvariantCulture);
                }
            }

			_options.ApplicationHttpUrl = url;

			if (url == null)
				throw new InvalidOperationException("No application URL has been set, and it could not be inferred.");

			return url;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_applicationLifetime.ApplicationStarted.Register(() => OnApplicationStarted());
		}

		public Task OnApplicationStarted()
		{
			var addressFeature = _server.Features.Get<IServerAddressesFeature>();

			_logger.LogDebug("Inferred hosting URLs as {ServerAddresses}.", addressFeature?.Addresses);
			_serverAddressesSource.SetResult(addressFeature?.Addresses.ToArray());

			return RunAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _processMgr.StopNGrokAsync();
		}
	}
}
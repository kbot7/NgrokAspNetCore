using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ngrok.AspNetCore.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Ngrok.ApiClient;
using Tunnel = Ngrok.ApiClient.Tunnel;
using Microsoft.Extensions.Options;

namespace Ngrok.AspNetCore
{
	class NgrokHostedService : INgrokHostedService
	{
		private readonly NgrokOptions _options;
		private readonly NgrokDownloader _nGrokDownloader;
		private readonly NgrokProcessMgr _processMgr;
		private readonly INgrokApiClient _client;
		private readonly IServer _server;
		private readonly IApplicationLifetime _applicationLifetime;

		private readonly TaskCompletionSource<IEnumerable<Tunnel>> _tunnelTaskCompletionSource;
		private IEnumerable<Tunnel> _tunnels;

		private readonly CancellationTokenSource _cancellationTokenSource;

		private ICollection<string> _addresses;

		public async Task<IEnumerable<Tunnel>> GetTunnelsAsync()
		{
			return await _tunnelTaskCompletionSource.Task;
		}

		public event Action<IEnumerable<Tunnel>> Ready;

		public NgrokHostedService(
			IOptionsMonitor<NgrokOptions> optionsMonitor,
			NgrokDownloader nGrokDownloader,
			IServer server,
			IApplicationLifetime applicationLifetime,
			NgrokProcessMgr processMgr,
			INgrokApiClient client)
		{
			_options = optionsMonitor.CurrentValue;
			_nGrokDownloader = nGrokDownloader;
			_server = server;
			_applicationLifetime = applicationLifetime;
			_processMgr = processMgr;
			_client = client;
			_tunnelTaskCompletionSource = new TaskCompletionSource<IEnumerable<Tunnel>>();
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			cancellationToken.Register(() => _cancellationTokenSource.Cancel());
			_applicationLifetime.ApplicationStarted.Register(() => OnApplicationStarted());
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			if (!_options.ManageNgrokProcess && _tunnels != null)
			{
				foreach (var tunnel in _tunnels)
				{
					await _client.StopTunnelAsync(tunnel.Name, cancellationToken);
				}
			}

			_cancellationTokenSource.Cancel();
			await _processMgr.StopNgrokAsync();
		}

		public Task OnApplicationStarted()
		{
			_addresses = _server.Features.Get<IServerAddressesFeature>().Addresses.ToArray();
			return RunAsync(_cancellationTokenSource.Token);
		}

		private async Task RunAsync(CancellationToken cancellationToken = default)
		{
			if (_options.DownloadNgrok)
			{
				await DownloadNgrokIfNeededAsync(cancellationToken);
			}

			if (_cancellationTokenSource.IsCancellationRequested)
				return;

			await _processMgr.EnsureNgrokStartedAsync(_options.NgrokPath, cancellationToken);

			if (_cancellationTokenSource.IsCancellationRequested)
				return;

			var url = AdjustApplicationHttpUrlIfNeeded();

			var tunnels = await StartTunnelsAsync(url, cancellationToken);
			OnTunnelsFetched(tunnels);
		}

		private void OnTunnelsFetched(IEnumerable<Tunnel> tunnels)
		{
			if (tunnels == null)
				throw new ArgumentNullException(nameof(tunnels), "Tunnels was not expected to be null here.");

			_tunnels = tunnels;
			_tunnelTaskCompletionSource.SetResult(tunnels);
			Ready?.Invoke(tunnels);
		}

		private async Task<IEnumerable<Tunnel>> StartTunnelsAsync(string address, CancellationToken cancellationToken)
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
			}, cancellationToken);

			// Get Tunnels
			var tunnels = (await _client.ListTunnelsAsync(cancellationToken))
				.Where(t => t.Name == System.AppDomain.CurrentDomain.FriendlyName ||
				t.Name == $"{System.AppDomain.CurrentDomain.FriendlyName} (http)");

			return tunnels;
		}

		private string AdjustApplicationHttpUrlIfNeeded()
		{
			var url = _options.ApplicationHttpUrl;

			if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
			{
				url = _addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? _addresses.FirstOrDefault();
			}

			_options.ApplicationHttpUrl = url;

			if (url == null)
				throw new InvalidOperationException("No application URL has been set, and it could not be inferred.");

			return url;
		}

		private async Task DownloadNgrokIfNeededAsync(CancellationToken cancellationToken = default)
		{
			var nGrokFullPath = await _nGrokDownloader.EnsureNgrokInstalled(_options, cancellationToken);
			_options.NgrokPath = nGrokFullPath;
		}

		
	}
}
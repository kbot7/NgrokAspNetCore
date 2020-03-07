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

		private readonly TaskCompletionSource<IEnumerable<Tunnel>> _tunnelTaskCompletionSource;

		private ICollection<string> _addresses;

		public async Task<IEnumerable<Tunnel>> GetTunnelsAsync()
		{
			return await _tunnelTaskCompletionSource.Task;
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

			_tunnelTaskCompletionSource = new TaskCompletionSource<IEnumerable<Tunnel>>();
		}


		private async Task RunAsync()
		{
			await DownloadNGrokIfNeededAsync();
			await _processMgr.EnsureNGrokStartedAsync(_options.NGrokPath);
			var url = AdjustApplicationHttpUrlIfNeeded();

			var tunnels = await StartTunnelsAsync(url);
			OnTunnelsFetched(tunnels);
		}

		private void OnTunnelsFetched(IEnumerable<Tunnel> tunnels)
		{
			if (tunnels == null)
				throw new ArgumentNullException(nameof(tunnels), "Tunnels was not expected to be null here.");

			_tunnelTaskCompletionSource.SetResult(tunnels);
			Ready?.Invoke(tunnels);
		}

		private async Task<IEnumerable<Tunnel>> StartTunnelsAsync(string address)
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

		private async Task DownloadNGrokIfNeededAsync()
		{
			var nGrokFullPath = await _nGrokDownloader.EnsureNGrokInstalled(_options);
			_options.NGrokPath = nGrokFullPath;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_applicationLifetime.ApplicationStarted.Register(() => OnApplicationStarted());
		}

		public Task OnApplicationStarted()
		{
			_addresses = _server.Features.Get<IServerAddressesFeature>().Addresses.ToArray();
			return RunAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _processMgr.StopNGrokAsync();
		}
	}
}
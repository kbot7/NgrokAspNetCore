using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.NGrokModels;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;

namespace FluffySpoon.AspNet.NGrok
{
    class NGrokHostedService : INGrokHostedService
    {
        private readonly NGrokLocalApiClient _localApiClient;
        private readonly NGrokOptions _options;
        private readonly NGrokDownloader _nGrokDownloader;
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
            NGrokLocalApiClient localApiClient,
            NGrokOptions options,
            NGrokDownloader nGrokDownloader,
            IServer server,
            IApplicationLifetime applicationLifetime)
        {
            _localApiClient = localApiClient;
            _options = options;
            _nGrokDownloader = nGrokDownloader;
            _server = server;
            _applicationLifetime = applicationLifetime;

            _tunnelTaskCompletionSource = new TaskCompletionSource<IEnumerable<Tunnel>>();
        }


        private async Task RunAsync()
        {
            await DownloadNGrokIfNeededAsync();
            var url = AdjustApplicationHttpUrlIfNeeded();

            var tunnels = await StartTunnelsAsync(url);
            OnTunnelsFetched(tunnels);
        }

        private void OnTunnelsFetched(Tunnel[] tunnels)
        {
            if (tunnels == null)
                throw new ArgumentNullException(nameof(tunnels), "Tunnels was not expected to be null here.");

            _tunnelTaskCompletionSource.SetResult(tunnels);
            Ready?.Invoke(tunnels);
        }

        private async Task<Tunnel[]> StartTunnelsAsync(string url)
        {
            var tunnels = await _localApiClient.StartTunnelsAsync(_options.NGrokPath, url);
            var tunnelsArray = tunnels?.ToArray();
            return tunnelsArray;
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
            _localApiClient.StopNGrok();
        }
    }
}

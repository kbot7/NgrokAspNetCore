using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NgrokAspNetCore.Lib.NgrokModels;
using NgrokAspNetCore.Lib.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;

namespace NgrokAspNetCore.Lib
{
    class NgrokHostedService : INgrokHostedService
    {
        private readonly NgrokLocalApiClient _localApiClient;
        private readonly NgrokOptions _options;
        private readonly NgrokDownloader _nGrokDownloader;
        private readonly IServer _server;
        private readonly IApplicationLifetime _applicationLifetime;

        private readonly TaskCompletionSource<IEnumerable<Tunnel>> _tunnelTaskCompletionSource;

        private ICollection<string> _addresses;

        public async Task<IEnumerable<Tunnel>> GetTunnelsAsync()
        {
            return await _tunnelTaskCompletionSource.Task;
        }

        public event Action<IEnumerable<Tunnel>> Ready;

        public NgrokHostedService(
            NgrokLocalApiClient localApiClient,
            NgrokOptions options,
            NgrokDownloader nGrokDownloader,
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
            await DownloadNgrokIfNeededAsync();
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
            var tunnels = await _localApiClient.StartTunnelsAsync(_options.NgrokPath, url);
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

        private async Task DownloadNgrokIfNeededAsync()
        {
            var nGrokFullPath = await _nGrokDownloader.EnsureNgrokInstalled(_options);
            _options.NgrokPath = nGrokFullPath;
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
            _localApiClient.StopNgrok();
        }
    }
}

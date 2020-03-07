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
using Microsoft.Extensions.Logging;

namespace FluffySpoon.AspNet.NGrok
{
    class NGrokHostedService : INGrokHostedService
    {
        private readonly NGrokLocalApiClient _localApiClient;
        private readonly NGrokOptions _options;
        private readonly NGrokDownloader _nGrokDownloader;
        private readonly ILogger<NGrokHostedService> _logger;

        private readonly TaskCompletionSource<IEnumerable<Tunnel>> _tunnelTaskCompletionSource;

        private string[] _serverAddresses;
        private Task _runTask;

        public async Task<IEnumerable<Tunnel>> GetTunnelsAsync()
        {
            RunIfNotAlreadyRunning();
            return await _tunnelTaskCompletionSource.Task;
        }

        public event Action<IEnumerable<Tunnel>> Ready;

        public NGrokHostedService(
            NGrokLocalApiClient localApiClient,
            NGrokOptions options,
            NGrokDownloader nGrokDownloader,
            ILogger<NGrokHostedService> logger)
        {
            _localApiClient = localApiClient;
            _options = options;
            _nGrokDownloader = nGrokDownloader;
            _logger = logger;

            _tunnelTaskCompletionSource = new TaskCompletionSource<IEnumerable<Tunnel>>();
        }

        public void InjectServerAddressesFeature(IServerAddressesFeature feature)
        {
            _serverAddresses = feature.Addresses.ToArray();
            _logger.LogDebug("Inferred hosting URLs as {ServerAddresses}.", new object[] {_serverAddresses});

            RunIfNotAlreadyRunning();
        }

        private void RunIfNotAlreadyRunning()
        {
            if (_runTask != null)
                return;

            _logger.LogTrace("Starting NGrok.");
            _runTask = RunAsync();
        }

        private async Task RunAsync()
        {
            await DownloadNGrokIfNeededAsync();

            var url = AdjustApplicationHttpUrlIfNeeded();
            _logger.LogInformation("Picked hosting URL {Url}.", url);

            var tunnels = await StartTunnelsAsync(url);
            _logger.LogInformation("Tunnels {Tunnels} have been started.", tunnels);

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
                var addresses = _serverAddresses;
                if(addresses != null)
                    url = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault();
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _localApiClient.StopNGrok();
        }
    }
}

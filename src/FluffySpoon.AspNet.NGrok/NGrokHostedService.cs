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
        private readonly NGrokApiClient _apiClient;
        private readonly NgrokOptions _options;
        private readonly NGrokDownloader _nGrokDownloader;
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
            NGrokApiClient apiClient,
            NgrokOptions options,
            NGrokDownloader nGrokDownloader,
            ILogger<NGrokHostedService> logger)
        {
            _apiClient = apiClient;
            _options = options;
            _nGrokDownloader = nGrokDownloader;
            _logger = logger;

            _tunnelTaskSource = new TaskCompletionSource<IReadOnlyCollection<Tunnel>>();
            _serverAddressesSource = new TaskCompletionSource<IReadOnlyCollection<string>>();

            RunAsync();
        }

        internal void InjectServerAddressesFeature(IServerAddressesFeature? feature)
        {
            _logger.LogDebug("Inferred hosting URLs as {ServerAddresses}.", feature?.Addresses);
            _serverAddressesSource.SetResult(feature?.Addresses.ToArray());
        }

        private async void RunAsync()
        {
            if (_options.Disable)
                return;

            await _nGrokDownloader.DownloadExecutableAsync();

            var url = await AdjustApplicationHttpUrlIfNeededAsync();
            _logger.LogInformation("Picked hosting URL {Url}.", url);

            var tunnels = await StartTunnelsAsync(url);
            _logger.LogInformation("Tunnels {Tunnels} have been started.", tunnels);

            OnTunnelsFetched(tunnels);
        }

        private void OnTunnelsFetched(Tunnel[] tunnels)
        {
            if (tunnels == null)
                throw new ArgumentNullException(nameof(tunnels), "Tunnels was not expected to be null here.");

            _tunnelTaskSource.SetResult(tunnels);
            Ready?.Invoke(tunnels);
        }

        private async Task<Tunnel[]> StartTunnelsAsync(string url)
        {
            var tunnels = await _apiClient.StartTunnelsAsync(url);
            var tunnelsArray = tunnels?.ToArray();
            return tunnelsArray;
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
                    $"No {nameof(NgrokOptions.ApplicationHttpUrl)} was set in the settings, and the URL of the server could not be inferred within 30 seconds. Perhaps you are missing a call to {nameof(NGrokAspNetCoreExtensions.UseNGrokAutomaticUrlDetection)} in your Configure method of your Startup class?");
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _apiClient.StopNGrok();
        }
    }
}

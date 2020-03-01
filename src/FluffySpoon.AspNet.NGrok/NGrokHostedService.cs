using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
    class NGrokHostedService : INGrokHostedService
    {
        private readonly NGrokLocalApiClient _localApiClient;
        private readonly NGrokOptions _options;
        private readonly NGrokDownloader _nGrokDownloader;

        private IServerAddressesFeature _serverAddressesFeature;

        public event Action Ready;

        public NGrokHostedService(
            NGrokLocalApiClient localApiClient,
            NGrokOptions options,
            NGrokDownloader nGrokDownloader)
        {
            _localApiClient = localApiClient;
            _options = options;
            _nGrokDownloader = nGrokDownloader;
        }

        public void InjectServerAddressesFeature(IServerAddressesFeature feature)
        {
            _serverAddressesFeature = feature;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (_serverAddressesFeature == null)
                await Task.Delay(50);

            var nGrokFullPath = await _nGrokDownloader.EnsureNGrokInstalled(_options);
            _options.NGrokPath = nGrokFullPath;

            var url = _options.ApplicationHttpUrl;
            var addresses = _serverAddressesFeature.Addresses;

            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                url = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault();
            }

            _options.ApplicationHttpUrl = url;

            await _localApiClient.StartTunnelsAsync(_options.NGrokPath, url);
            Ready?.Invoke();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _localApiClient.StopNGrok();
        }
    }
}

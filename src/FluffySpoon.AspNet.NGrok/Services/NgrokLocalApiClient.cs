// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 David Prothero, Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.Exceptions;
using FluffySpoon.AspNet.NGrok.NGrokModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluffySpoon.AspNet.NGrok.Services
{
    public class NGrokLocalApiClient
    {
        private readonly HttpClient _nGrokApi;
        private readonly ILogger _logger;
        private readonly NGrokProcess _nGrokProcess;
        private readonly NGrokOptions _options;

        public NGrokLocalApiClient(HttpClient httpClient, NGrokProcess nGrokProcess, NGrokOptions options, ILogger<NGrokLocalApiClient> logger)
        {
            _nGrokApi = httpClient;
            _options = options;
            _nGrokProcess = nGrokProcess;
            _logger = logger;

            // TODO some of this can be moved to the DI registration
            _nGrokApi.BaseAddress = new Uri("http://localhost:4040");
            _nGrokApi.DefaultRequestHeaders.Accept.Clear();
            _nGrokApi.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nGrokPath"></param>
        /// <exception cref="NGrokStartFailedException">Throws when NGrok failed to start</exception>
        /// <exception cref="NGrokUnsupportedException">Throws when NGrok is not suported on the os and architecture</exception>
        /// <exception cref="NGrokNotFoundException">Throws when NGrok is not found and is unable to be downloaded automatically</exception>
        /// <returns></returns>
        internal async Task<IEnumerable<Tunnel>> StartTunnelsAsync(string nGrokPath, string url)
        {
            await StartNGrokAsync(nGrokPath);

            if (await HasTunnelByAddressAsync(url))
                return await GetTunnelListAsync();

            var tunnel = await CreateTunnelAsync(System.AppDomain.CurrentDomain.FriendlyName, url);

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
            {
                await Task.Delay(100);

                if (await HasTunnelByAddressAsync(tunnel?.Config?.Addr))
                    return await GetTunnelListAsync();
            }

            throw new Exception("A timeout occured while waiting for the created tunnel to exist.");
        }

        private async Task<bool> HasTunnelByAddressAsync(string address)
        {
            var tunnels = await GetTunnelListAsync();
            return tunnels != null && tunnels.Any(x => x.Config?.Addr == address);
        }

        /// <returns></returns>
        private async Task StartNGrokAsync(string nGrokPath)
        {
            // This allows a pre-existing NGrok instance to be used, instead of the one we are starting here. 
            if (await CanGetTunnelList())
                return;

            try
            {
                _nGrokProcess.StartNGrokProcess(nGrokPath);

                var stopwatch = Stopwatch.StartNew();
                var canGetTunnelList = false;
                while (!canGetTunnelList && stopwatch.Elapsed < TimeSpan.FromSeconds(30))
                {
                    canGetTunnelList = await CanGetTunnelList();
                    await Task.Delay(100);
                }

                if (!canGetTunnelList)
                {
                    throw new Exception("A timeout occured while waiting for the NGrok process.");
                }
            }
            catch (Exception ex)
            {
                throw new NGrokStartFailedException(ex);
            }
        }

        internal void StopNGrok()
        {
            _nGrokProcess.Stop();
        }

        private async Task<bool> CanGetTunnelList()
        {
            var tunnels = await GetTunnelListAsync();
            return tunnels != null;
        }

        internal async Task<Tunnel[]> GetTunnelListAsync()
        {
            try
            {
                var response = await _nGrokApi.GetAsync("/api/tunnels");
                if (!response.IsSuccessStatusCode)
                    return null;

                var responseText = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<NGrokTunnelsApiResponse>(responseText);
                return apiResponse.Tunnels;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private async Task<Tunnel> CreateTunnelAsync(string projectName, string address)
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

            var request = new NGrokTunnelApiRequest
            {
                Name = projectName,
                Addr = address,
                Proto = "http",
                HostHeader = address
            };

            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            Debug.WriteLine($"request: '{json}'");

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _nGrokApi.PostAsync("/api/tunnels", httpContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"{response.StatusCode} errorText: '{errorText}'");

                var error = JsonConvert.DeserializeObject<NGrokErrorApiResult>(errorText);
                _logger.Log(LogLevel.Error,
                    $"Could not create tunnel for {projectName} ({address}): " +
                    $"\n[{error.ErrorCode}] {error.Msg}" +
                    $"\nDetails: {error.Details.Err.Replace("\\n", "\n")}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"responseText: '{responseText}'");
            return JsonConvert.DeserializeObject<Tunnel>(responseText);
        }
    }
}

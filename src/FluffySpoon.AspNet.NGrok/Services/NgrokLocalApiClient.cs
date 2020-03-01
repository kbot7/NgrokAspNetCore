// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 David Prothero, Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
		private Tunnel[] _tunnels;

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
		public async Task<IEnumerable<Tunnel>> StartTunnelsAsync(string nGrokPath, string url)
		{
            await StartNGrokAsync(nGrokPath);
            return await StartNGrokTunnelAsync(System.AppDomain.CurrentDomain.FriendlyName, url);
		}

		/// <returns></returns>
		private async Task StartNGrokAsync(string nGrokPath)
		{
			// This allows a pre-existing NGrok instance to be used, instead of the one we are starting here. 
			if (await CanGetTunnelList()) return;

			try
			{
				_nGrokProcess.StartNGrokProcess(nGrokPath);

				// This is accomplishing a retry and delay for checking if we can get tunnels
				// It is also ensuring NGrok is up by waiting 250 ms and hoping NGrok has started in that time
				// TODO replace by polling local API until it is up with http client using very short (<25ms) timeouts. Should be quicker than waiting 50 arbitrary ms. Limit retry attempts to 3
				await Task.Delay(50);
				if (await CanGetTunnelList()) return;
			}
			catch (Exception ex)
			{
				throw new NGrokStartFailedException(ex);
			}
		}

		public Task StopNGrok()
		{
			_nGrokProcess.Stop();
			return Task.CompletedTask;
		}

		private async Task<bool> CanGetTunnelList()
		{
			try
			{
				_tunnels = await GetTunnelListAsync();
			}
			catch
			{
				//ignored.
			}
			return (_tunnels != null);
		}

		public async Task<Tunnel[]> GetTunnelListAsync()
		{
			var response = await _nGrokApi.GetAsync("/api/tunnels");
			if (response.IsSuccessStatusCode)
			{
				var responseText = await response.Content.ReadAsStringAsync();
				NGrokTunnelsApiResponse apiResponse = null;
				try
				{
					apiResponse = JsonConvert.DeserializeObject<NGrokTunnelsApiResponse>(responseText);
				}
				catch (Exception ex)
				{
					_logger.LogError("Failed to deserialize NGrok tunnel response");
					throw ex;
				}

				return apiResponse.Tunnels;
			}
			return null;
		}
		private async Task<IEnumerable<Tunnel>> StartNGrokTunnelAsync(string projectName, string addr)
		{
            var existingTunnels = _tunnels.Where(t => t.Config.Addr == addr);

			if (existingTunnels.Any())
			{
				_logger.Log(LogLevel.Information, "Existing tunnels: {@tunnels}", existingTunnels);
				return existingTunnels;
			}
			else
			{
				var tunnel = await CreateTunnelAsync(projectName, addr);
				return tunnel.SingleItemAsEnumerable();
			}
		}

		private async Task<Tunnel> CreateTunnelAsync(string projectName, string addr, bool retry = false)
		{
			if ( string.IsNullOrEmpty(addr))
			{
				addr = "80";
			}
			else
			{
				int x;
				if ( !int.TryParse(addr, out x))
				{
					var url = new Uri(addr);
					if (url.Port != 80 && url.Port != 443)
					{
						addr = $"{url.Host}:{url.Port}";
					}
					else
					{
						if (addr.StartsWith("http://"))
						{
							addr = addr.Remove(addr.IndexOf("http://"), "http://".Length);
						}
						if (addr.StartsWith("https://"))
						{
							addr = addr.Remove(addr.IndexOf("https://"), "https://".Length);
						}
					}
				}
			}

			var request = new NGrokTunnelApiRequest
			{
				Name = projectName,
				Addr = addr,
				Proto = "http",
				HostHeader = addr
			};

			// TODO fix later once I bring back support for NGrok configs
			//if (!string.IsNullOrEmpty(_webAppConfig.SubDomain))
			//{
			//	request.subdomain = _webAppConfig.SubDomain;
			//}

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
				NGrokErrorApiResult error;

				try
				{
					error = JsonConvert.DeserializeObject<NGrokErrorApiResult>(errorText);
				}
				catch (JsonReaderException)
				{
					error = null;
				}

				if (error != null)
				{
					_logger.Log(LogLevel.Error, $"Could not create tunnel for {projectName} ({addr}): " +
										 $"\n[{error.ErrorCode}] {error.Msg}" +
										 $"\nDetails: {error.Details.Err.Replace("\\n", "\n")}");
				}
				else
				{
					if (retry)
					{
						_logger.Log(LogLevel.Error, $"Could not create tunnel for {projectName} ({addr}): " +
											 $"\n{errorText}");
					}
					else
					{
						await Task.Delay(1000);  // wait for NGrok to spin up completely?
						await CreateTunnelAsync(projectName, addr, true);
					}
				}
				return null;
			}

			var responseText = await response.Content.ReadAsStringAsync();
			Debug.WriteLine($"responseText: '{responseText}'");
			return JsonConvert.DeserializeObject<Tunnel>(responseText);
		}
	}
	public static class EnumerableExt
	{
		// usage: someObject.SingleItemAsEnumerable();
		public static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
		{
			yield return item;
		}
	}
}

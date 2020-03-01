// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 David Prothero, Kevin Gysberg

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NgrokExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NgrokAspNetCore
{
	public class NgrokLocalApiClient
	{
		private readonly HttpClient _ngrokApi;
		private readonly ILogger _logger;
		private readonly NgrokProcess _ngrokProcess;
		private readonly NgrokOptions _options;
		private Tunnel[] _tunnels;

		public NgrokLocalApiClient(HttpClient httpClient, NgrokProcess ngrokProcess, NgrokOptions options, ILogger<NgrokLocalApiClient> logger)
		{
			_ngrokApi = httpClient;
			_options = options;
			_ngrokProcess = ngrokProcess;
			_logger = logger;

			// TODO some of this can be moved to the DI registration
			_ngrokApi.BaseAddress = new Uri("http://localhost:4040");
			_ngrokApi.DefaultRequestHeaders.Accept.Clear();
			_ngrokApi.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ngrokPath"></param>
		/// <exception cref="NgrokStartFailedException">Throws when ngrok failed to start</exception>
		/// <exception cref="NgrokUnsupportedException">Throws when ngrok is not suported on the os and architecture</exception>
		/// <exception cref="NgrokNotFoundException">Throws when ngrok is not found and is unable to be downloaded automatically</exception>
		/// <returns></returns>
		public async Task<IEnumerable<Tunnel>> StartTunnelsAsync(string ngrokPath)
		{
			return await DoStartTunnelsAsync(ngrokPath);
		}

		private async Task<IEnumerable<Tunnel>> DoStartTunnelsAsync(string ngrokPath)
		{
			await StartNgrokAsync(ngrokPath);
			return await StartNgrokTunnelAsync(System.AppDomain.CurrentDomain.FriendlyName);
		}

		/// <returns></returns>
		private async Task StartNgrokAsync(string ngrokPath, bool retry = false)
		{
			// This allows a pre-existing ngrok instance to be used, instead of the one we are starting here. 
			if (await CanGetTunnelList()) return;

			try
			{
				_ngrokProcess.StartNgrokProcess(ngrokPath);

				// This is accomplishing a retry and delay for checking if we can get tunnels
				// It is also ensuring ngrok is up by waiting 250 ms and hoping ngrok has started in that time
				// TODO replace by polling local API until it is up with http client using very short (<25ms) timeouts. Should be quicker than waiting 250 arbitrary ms. Limit retry attempts to 3
				await Task.Delay(250);
				if (await CanGetTunnelList(retry: true)) return;
			}
			catch (Exception ex)
			{
				throw new NgrokStartFailedException(ex);
			}
		}

		public Task StopNgrok()
		{
			_ngrokProcess.Stop();
			return Task.CompletedTask;
		}

		private async Task<bool> CanGetTunnelList(bool retry = false)
		{
			try
			{
				_tunnels = await GetTunnelListAsync();
			}
			catch (Exception)
			{
				if (retry) throw;
			}
			return (_tunnels != null);
		}

		public async Task<Tunnel[]> GetTunnelListAsync()
		{
			var response = await _ngrokApi.GetAsync("/api/tunnels");
			if (response.IsSuccessStatusCode)
			{
				var responseText = await response.Content.ReadAsStringAsync();
				NgrokTunnelsApiResponse apiResponse = null;
				try
				{
					apiResponse = JsonConvert.DeserializeObject<NgrokTunnelsApiResponse>(responseText);
				}
				catch (Exception ex)
				{
					_logger.LogError("Failed to deserialize ngrok tunnel response");
					throw ex;
				}

				return apiResponse.tunnels;
			}
			return null;
		}
		private async Task<IEnumerable<Tunnel>> StartNgrokTunnelAsync(string projectName)
		{
			var addr = _options.ApplicationHttpUrl;

			var existingTunnels = _tunnels.Where(t => t.config.addr == addr);

			if (existingTunnels.Any())
			{
				_logger.Log(LogLevel.Information, "Existing tunnels: {@tunnels}", existingTunnels);
				return existingTunnels;
			}
			else
			{
				var tunnel = await CreateTunnelAsync(projectName, addr);
				return IEnumerableExt.SingleItemAsEnumerable(tunnel);
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

			var request = new NgrokTunnelApiRequest
			{
				name = projectName,
				addr = addr,
				proto = "http",
				host_header = addr
			};

			// TODO fix later once I bring back support for ngrok configs
			//if (!string.IsNullOrEmpty(_webAppConfig.SubDomain))
			//{
			//	request.subdomain = _webAppConfig.SubDomain;
			//}

			Debug.WriteLine($"request: '{JsonConvert.SerializeObject(request)}'");
			var json = JsonConvert.SerializeObject(request);
			var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await _ngrokApi.PostAsync("/api/tunnels", httpContent);
			if (!response.IsSuccessStatusCode)
			{
				var errorText = await response.Content.ReadAsStringAsync();
				Debug.WriteLine($"{response.StatusCode} errorText: '{errorText}'");
				NgrokErrorApiResult error;

				try
				{
					error = JsonConvert.DeserializeObject<NgrokErrorApiResult>(errorText);
				}
				catch (JsonReaderException)
				{
					error = null;
				}

				if (error != null)
				{
					_logger.Log(LogLevel.Error, $"Could not create tunnel for {projectName} ({addr}): " +
										 $"\n[{error.error_code}] {error.msg}" +
										 $"\nDetails: {error.details.err.Replace("\\n", "\n")}");
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
						await Task.Delay(1000);  // wait for ngrok to spin up completely?
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
	public static class IEnumerableExt
	{
		// usage: someObject.SingleItemAsEnumerable();
		public static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
		{
			yield return item;
		}
	}
}

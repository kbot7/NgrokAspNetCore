using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.ApiClient
{
	public class NgrokHttpClient : INgrokApiClient
	{
		private const string ListTunnelsPath = "/api/tunnels";
		private const string GetTunnelPathFormat = "/api/tunnels/{0}";
		private const string StartTunnelPath = "/api/tunnels";
		private const string StopTunnelPathFormat = "/api/tunnels/{0}";

		public HttpClient Client { get; }

		public NgrokHttpClient(HttpClient client)
		{
			client.BaseAddress = new Uri("http://localhost:4040");

			// TODO set content-type to application/json

			Client = client;
		}

		public async Task<IEnumerable<Tunnel>> ListTunnelsAsync(CancellationToken cancellationToken = default)
		{
			var response = await Client.GetAsync(ListTunnelsPath);
			await ThrowIfError(response);

			using var responseStream = await response.Content.ReadAsStreamAsync();
			var listTunnelResponse = await JsonSerializer.DeserializeAsync
				<ListTunnelsResponse>(responseStream);
			return listTunnelResponse.Tunnels;
		}

		public async Task<Tunnel> StartTunnelAsync(StartTunnelRequest request, CancellationToken cancellationToken = default)
		{
			HttpResponseMessage response;
			using (var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"))
			{
				response = await Client.PostAsync(StartTunnelPath, content);
			}
			await ThrowIfError(response);

			using var responseStream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync
				<Tunnel>(responseStream);
		}

		public async Task<Tunnel> GetTunnelAsync(string name, CancellationToken cancellationToken = default)
		{
			var response = await Client.GetAsync(string.Format(GetTunnelPathFormat, name));
			await ThrowIfError(response);

			using var responseStream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync
				<Tunnel>(responseStream);
		}

		public async Task StopTunnelAsync(string name, CancellationToken cancellationToken = default)
		{
			var response = await Client.DeleteAsync(string.Format(StopTunnelPathFormat, name));
			await ThrowIfError(response);
		}

		private async Task ThrowIfError(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				using var responseStream = await response.Content.ReadAsStreamAsync();
				var errorResponse = await JsonSerializer.DeserializeAsync
					<ErrorResponse>(responseStream);
				throw new NgrokApiException(errorResponse);
			}
		}
	}
}

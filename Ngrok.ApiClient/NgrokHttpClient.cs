using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.ApiClient
{
	public class NgrokHttpClient : INgrokApiClient
	{
		public HttpClient Client { get; }

		public NgrokHttpClient(HttpClient client)
		{
			client.BaseAddress = new Uri("http://localhost:4040");

			// TODO set content-type to application/json

			Client = client;
		}

		public Task<ListTunnelsResponse> ListTunnelsAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Tunnel> StartTunnelAsync(StartTunnelRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Tunnel> GetTunnelAsync(string name, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopTunnelAsync(string name, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}

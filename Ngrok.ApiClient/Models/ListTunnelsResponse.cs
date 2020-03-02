using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{

	public class ListTunnelsResponse
	{
		[JsonPropertyName("tunnels")]
		public IEnumerable<Tunnel> Tunnels { get; set; }

		[JsonPropertyName("uri")]
		public string Uri { get; set; }
	}

}
using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class StartTunnelRequest
	{
		/// <summary>
		/// Tunnel protocol name, one of http, tcp, tls
		/// </summary>
		[JsonPropertyName("proto")] // Required
		public string Protocol { get; set; } // TODO make into enum

		/// <summary>
		/// Forward traffic to this local port number or network address
		/// </summary>
		[JsonPropertyName("addr")] // Required
		public string Address { get; set; }

		/// <summary>
		/// Name of the tunnel
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Enable http request inspection
		/// </summary>
		[JsonPropertyName("inspect")]
		public bool Inspect { get; set; }

		// TODO use strong-type for Username and Pass on C# side instead of string
		[JsonPropertyName("auth")]
		public string Auth { get; set; }

		[JsonPropertyName("host_header")]
		public string HostHeader { get; set; }

		// TODO make into enum
		[JsonPropertyName("bind_tls")] // 'true', 'false', or 'both'
		public string BindTLS { get; set; }

		[JsonPropertyName("subdomain")]
		public string Subdomain { get; set; }



		
	}
}
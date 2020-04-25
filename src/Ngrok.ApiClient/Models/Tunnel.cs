using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class Tunnel
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("uri")]
		public string URI { get; set; }

		[JsonPropertyName("public_url")]
		public string PublicURL { get; set; }

		[JsonPropertyName("proto")]
		public string Proto { get; set; }

		[JsonPropertyName("config")]
		public TunnelConfig Config { get; set; }

		[JsonPropertyName("metrics")]
		public TunnelMetrics Metrics { get; set; }
	}

}
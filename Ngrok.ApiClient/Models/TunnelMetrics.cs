using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class TunnelMetrics
	{
		[JsonPropertyName("conns")]
		public TunnelConnectionMetrics ConnectionMetrics { get; set; }

		[JsonPropertyName("http")]
		public TunnelHttpMetrics HttpMetrics { get; set; }
	}

}
using System.Text.Json.Serialization;

namespace NGrok.ApiClient
{
	public class TunnelMetrics
	{
		[JsonPropertyName("conns")]
		public TunnelConnectionMetrics ConnectionMetrics { get; set; }

		[JsonPropertyName("http")]
		public TunnelHttpMetrics HttpMetrics { get; set; }
	}

}
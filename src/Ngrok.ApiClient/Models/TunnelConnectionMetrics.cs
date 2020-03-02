using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class TunnelConnectionMetrics
	{
		[JsonPropertyName("count")]
		public int Count { get; set; }

		[JsonPropertyName("gauge")]
		public int Gauge { get; set; }

		[JsonPropertyName("rate1")]
		public decimal Rate1 { get; set; }

		[JsonPropertyName("rate5")]
		public decimal Rate5 { get; set; }

		[JsonPropertyName("rate15")]
		public decimal Rate15 { get; set; }

		[JsonPropertyName("p50")]
		public decimal P50 { get; set; }

		[JsonPropertyName("p90")]
		public decimal P90 { get; set; }

		[JsonPropertyName("p95")]
		public decimal P95 { get; set; }

		[JsonPropertyName("p99")]
		public decimal P99 { get; set; }
	}

}
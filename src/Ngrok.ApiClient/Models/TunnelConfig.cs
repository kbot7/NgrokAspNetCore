using System.Text.Json.Serialization;

namespace NGrok.ApiClient
{
	public class TunnelConfig
	{
		[JsonPropertyName("addr")]
		public string Address { get; set; }

		[JsonPropertyName("inspect")]
		public bool Inspect { get; set; }
	}
}
using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class TunnelConfig
	{
		[JsonPropertyName("addr")]
		public string Address { get; set; }

		[JsonPropertyName("inspect")]
		public bool Inspect { get; set; }
	}
}
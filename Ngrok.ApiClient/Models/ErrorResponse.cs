using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class ErrorResponse
	{
		[JsonPropertyName("error_code")]
		public int ErrorCode { get; set; }

		[JsonPropertyName("status_code")]
		public int StatusCode { get; set; }

		[JsonPropertyName("msg")]
		public string Message { get; set; }

		[JsonPropertyName("details")]
		public Details Details { get; set; }
	}

	public class Details
	{
		[JsonPropertyName("err")]
		public string Error { get; set; }
	}
}
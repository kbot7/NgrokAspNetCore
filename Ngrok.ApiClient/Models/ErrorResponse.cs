using System.Text.Json.Serialization;

namespace Ngrok.ApiClient
{
	public class ErrorResponse
	{
		[JsonPropertyName("error_code")]
		public int NgrokErrorCode { get; set; }

		[JsonPropertyName("status_code")]
		public int HttpStatusCode { get; set; }

		[JsonPropertyName("msg")]
		public string Message { get; set; }

		[JsonPropertyName("details")]
		public Details Details { get; set; }
	}

	public class Details
	{
		[JsonPropertyName("err")]
		public string DetailedErrorMessage { get; set; }
	}
}
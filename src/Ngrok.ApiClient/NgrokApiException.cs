using System;

namespace Ngrok.ApiClient
{
	public class NgrokApiException : Exception
	{
		private const string MessageFormat =
			"Error calling Ngok Local API | HttpStatusCode: {0} | NgrokErrorCode: {1} | Message: {2} | DetailedMessage: {3}";
		public NgrokApiException(ErrorResponse error)
			: base(string.Format(MessageFormat, error.HttpStatusCode, error.NgrokErrorCode, error.Message, error.Details.DetailedErrorMessage)) { }
	}
}
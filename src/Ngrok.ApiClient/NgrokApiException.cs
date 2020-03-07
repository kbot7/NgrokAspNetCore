using System;

namespace NGrok.ApiClient
{
	public class NGrokApiException : Exception
	{
		private const string MessageFormat =
			"Error calling Ngok Local API | HttpStatusCode: {0} | NGrokErrorCode: {1} | Message: {2} | DetailedMessage: {3}";
		public NGrokApiException(ErrorResponse error)
			: base(string.Format(MessageFormat, error.HttpStatusCode, error.NGrokErrorCode, error.Message, error.Details.DetailedErrorMessage)) { }
	}
}
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;

namespace Ngrok.AspNetCore.Exceptions
{
	[Serializable]
	internal class NgrokConfigurationError : Exception
	{
		public NgrokConfigurationError() : base("Ngrok configuration failed")
		{
		}

		public NgrokConfigurationError(string message) : base($"Ngrok configuration failed. {message}")
		{
		}

		public NgrokConfigurationError(Exception innerException) : base("Ngrok failed to start", innerException)
		{
		}
	}
}
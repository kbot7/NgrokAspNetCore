// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;

namespace FluffySpoon.AspNet.NGrok.Exceptions
{
	[Serializable]
	internal class NgrokStartFailedException : Exception
	{
		public NgrokStartFailedException() : base("ngrok failed to start")
		{
		}

		public NgrokStartFailedException(Exception innerException) : base("ngrok failed to start", innerException)
		{
		}
	}
}
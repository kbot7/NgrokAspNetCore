// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Runtime.Serialization;

namespace NgrokAspNetCore.Lib
{
	[Serializable]
	internal class NgrokStartFailedException : Exception
	{
		public NgrokStartFailedException() : base("ngrok.exe failed to start")
		{
		}

		public NgrokStartFailedException(Exception innerException) : base("ngrok.exe failed to start", innerException)
		{
		}
	}
}
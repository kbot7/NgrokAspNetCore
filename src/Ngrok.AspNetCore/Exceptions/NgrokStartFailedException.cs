// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;

namespace Ngrok.AspNetCore.Exceptions
{
	[Serializable]
	internal class NGrokStartFailedException : Exception
	{
		public NGrokStartFailedException() : base("NGrok failed to start")
		{
		}

		public NGrokStartFailedException(Exception innerException) : base("NGrok failed to start", innerException)
		{
		}
	}
}
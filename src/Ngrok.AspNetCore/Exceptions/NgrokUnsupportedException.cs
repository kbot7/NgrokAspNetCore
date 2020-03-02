// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Runtime.InteropServices;

namespace Ngrok.AspNetCore.Exceptions
{
	public class NgrokUnsupportedException : Exception
	{
		public NgrokUnsupportedException() : base($"Platform not supported by Ngrok {RuntimeInformation.OSDescription}-{RuntimeInformation.ProcessArchitecture}")
		{
		}
	}
}
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NgrokAspNetCore.Lib
{
	public class NgrokUnsupportedException : Exception
	{
		public NgrokUnsupportedException() : base($"Platform not supported by ngrok {RuntimeInformation.OSDescription}-{RuntimeInformation.ProcessArchitecture}")
		{
		}
	}
}

// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;
using System.Runtime.InteropServices;

namespace FluffySpoon.AspNet.NGrok.Exceptions
{
	public class NGrokUnsupportedException : Exception
	{
		public NGrokUnsupportedException() : base($"Platform not supported by NGrok {RuntimeInformation.OSDescription}-{RuntimeInformation.ProcessArchitecture}")
		{
		}
	}
}
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;

namespace FluffySpoon.AspNet.NGrok.Exceptions
{
	public class NgrokNotFoundException : Exception
	{
		public NgrokNotFoundException() : base("Ngrok not found in current directory, or PATH. If download attempted, it failed. Please download ngrok manually and place in running directory. If already downloaded, configure path in NgrokOptions")
		{ }
	}
}

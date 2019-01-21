// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System;

namespace NgrokAspNetCore
{
	public class NgrokNotFoundException : Exception
	{
		public NgrokNotFoundException() : base("Ngrok not found in current directory, or Windows PATH. If download attempted, it failed. Please download ngrok.exe manually and place in running directory. If already downloaded, configure path in NgrokOptions")
		{ }
	}
}

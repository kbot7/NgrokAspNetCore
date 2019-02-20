// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

namespace NgrokAspNetCore
{
	public class NgrokOptions
	{
		/// <summary>
		/// Path to Ngrok executable. If not set, the execution directory and Windows PATH (if on Windows) will be searched
		/// </summary>
		public string NgrokPath { get; set; }

		/// <summary>
		/// Use a named Ngrok profile. An ngrok config file must be present in the execution direction, or one must be specified in the <see cref="NgrokConfigPath"/>
		/// See <see href="https://ngrok.com/docs#config">https://ngrok.com/docs#config</see> for details
		/// </summary>
		public string NgrokConfigProfile { get; set; }

		/// <summary>
		/// Path to the ngrok config if <see cref="NgrokConfigProfile"/> is specified. Can be relative to execution directory, or fully qualified
		/// </summary>
		public string NgrokConfigPath { get; set; }

		/// <summary>
		/// Sets the local URL Ngrok will proxy to. Must be http (not https) at this time. If not filled in, it will be populated automatically at runtime via the IWebHost features
		/// </summary>
		public string ApplicationHttpUrl { get; set; }

		/// <summary>
		/// Download ngrok if not found in local directory, configuration, or PATH. Defaults to true
		/// </summary>
		public bool DownloadNgrok { get; set; } = true;
	}
}

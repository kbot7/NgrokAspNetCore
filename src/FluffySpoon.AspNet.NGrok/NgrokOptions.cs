// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

namespace FluffySpoon.AspNet.NGrok
{
	public class NGrokOptions
	{
		/// <summary>
		/// Path to NGrok executable. If not set, the execution directory and Windows PATH (if on Windows) will be searched
		/// </summary>
		public string NGrokPath { get; set; }

		/// <summary>
		/// Set to NGrok profile specified in NGrok config. NGrok config file must be present to use this option
		/// See <see href="https://NGrok.com/docs#config">https://NGrok.com/docs#config</see> for details
		/// </summary>
		public string NGrokConfigProfile { get; set; }

		/// <summary>
		/// Sets the local URL NGrok will proxy to. Must be http (not https) at this time. If not filled in, it will be populated automatically at runtime via the IWebHost features
		/// </summary>
		public string ApplicationHttpUrl { get; set; }

		/// <summary>
		/// Download NGrok if not found in local directory or PATH. Defaults to true
		/// </summary>
		public bool DownloadNGrok { get; set; } = true;
	}
}

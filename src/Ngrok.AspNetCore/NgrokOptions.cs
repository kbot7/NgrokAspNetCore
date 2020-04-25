// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

namespace Ngrok.AspNetCore
{
	public class NgrokOptions
	{
		/// <summary>
		/// Automatically detect URL from <see cref="Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature"/>
		/// </summary>
		public bool DetectUrl { get; set; } = true;

		/// <summary>
		/// Sets the local URL Ngrok will proxy to. Must be http (not https) at this time. 
		/// Implies <see cref="DetectUrl"/> is false
		/// </summary>
		public string ApplicationHttpUrl { get; set; }

		/// <summary>
		/// Launch and manage an ngrok process if one is not already running on startup
		/// </summary>
		public bool ManageNgrokProcess { get; set; } = true;

		/// <summary>
		/// Path to the Ngrok executable. If not set, the execution directory and PATH will be searched. 
		/// Implies <see cref="ManageNgrokProcess"/> is true
		/// </summary>
		public string NgrokPath { get; set; }

		/// <summary>
		/// Download Ngrok if not found in local directory or PATH. 
		/// Implies <see cref="ManageNgrokProcess"/> is true
		/// </summary>
		public bool DownloadNgrok { get; set; } = true;

		/// <summary>
		/// Time in milliseconds to wait for the ngrok process to start. 
		/// Implies <see cref="ManageNgrokProcess"/> is true. 
		/// Default is 5 seconds
		/// </summary>
		public int ProcessStartTimeoutMs { get; set; } = 5000;

		/// <summary>
		/// Redirect Ngrok process logs to Microsoft.Extensions.Logging. 
		/// Implies <see cref="ManageNgrokProcess"/> is true
		/// </summary>
		public bool RedirectLogs { get; set; } = true;

		/// <summary>
		/// Display a separate ngrok window
		/// Implies <see cref="ManageNgrokProcess"/> is true
		/// </summary>
		public bool DisplayNgrokWindow { get; set; } = false;

		/// <summary>
		/// Set to Ngrok profile specified in Ngrok config. Ngrok config file must be present to use this option
		/// See <see href="https://Ngrok.com/docs#config">https://Ngrok.com/docs#config</see> for details
		/// </summary>
		public string NgrokConfigProfile { get; set; }
	}
}
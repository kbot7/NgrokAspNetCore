// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NgrokExtensions

namespace NgrokAspNetCore.Lib.NgrokModels
{
	public class NgrokTunnelApiRequest
	{
		public string Name { get; set; }
		public string Addr { get; set; }
		public string Proto { get; set; }
		public string Subdomain { get; set; }
		public string HostHeader { get; set; }
	}
}
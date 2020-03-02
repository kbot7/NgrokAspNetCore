
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NgrokExtensions

namespace NgrokAspNetCore.Lib.NgrokModels
{
	public class NgrokErrorApiResult
	{
		public int ErrorCode { get; set; }
		public int StatusCode { get; set; }
		public string Msg { get; set; }
		public Details Details { get; set; }
	}

	public class Details
	{
		public string Err { get; set; }
	}
}

// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NGrokExtensions

using Newtonsoft.Json;

namespace FluffySpoon.AspNet.NGrok.NGrokModels
{
	public class NGrokErrorApiResult
	{
		[JsonProperty("error_code")]
		public int ErrorCode { get; set; }

        [JsonProperty("status_code")]
		public int StatusCode { get; set; }

        [JsonProperty("msg")]
		public string Message { get; set; }

		public Details Details { get; set; }
	}

	public class Details
	{
        [JsonProperty("err")]
		public string ErrorMessage { get; set; }
	}
}
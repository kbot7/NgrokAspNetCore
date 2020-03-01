// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NGrokExtensions

using Newtonsoft.Json;

namespace FluffySpoon.AspNet.NGrok.NGrokModels
{

	public class NGrokTunnelsApiResponse
	{
		public Tunnel[] Tunnels { get; set; }
		public string Uri { get; set; }
	}

	public class Tunnel
	{
		public string Name { get; set; }
		public string Uri { get; set; }

		[JsonProperty("public_url")]
		public string PublicUrl { get; set; }

		public string Proto { get; set; }
		public Config Config { get; set; }
		public Metrics Metrics { get; set; }
	}

	public class Config
	{
		public string Addr { get; set; }
		public bool Inspect { get; set; }
	}

	public class Metrics
	{
		public Conns Conns { get; set; }
		public Http Http { get; set; }
	}

	public class Conns
	{
		public int Count { get; set; }
		public int Gauge { get; set; }
		public decimal Rate1 { get; set; }
		public decimal Rate5 { get; set; }
		public decimal Rate15 { get; set; }
		public decimal P50 { get; set; }
		public decimal P90 { get; set; }
		public decimal P95 { get; set; }
		public decimal P99 { get; set; }
	}

	public class Http
	{
		public int Count { get; set; }
		public decimal Rate1 { get; set; }
		public decimal Rate5 { get; set; }
		public decimal Rate15 { get; set; }
		public decimal P50 { get; set; }
		public decimal P90 { get; set; }
		public decimal P95 { get; set; }
		public decimal P99 { get; set; }
	}

}
using Ngrok.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace NGrok.AspNetCore.UnitTests
{
	public class NGrokLogExtensionsTests
	{
		[Fact]
		public void ParseStartTunnel()
		{
			var input = "t=2020-03-01T23:36:39-0600 lvl=info msg=\"started tunnel\" obj=tunnels name=test_tunnel addr=http://localhost:425 url=https://12030abc.ngrok.io";
			var expectedResult = new Dictionary<string, string>()
			{
				{ "t", "2020-03-01T23:36:39-0600" },
				{ "lvl", "info" },
				{ "msg", "started tunnel" },
				{ "obj", "tunnels" },
				{ "name", "test_tunnel" },
				{ "addr", "http://localhost:425" },
				{ "url", "https://12030abc.ngrok.io" }
			};
			var result = NGrokLogExtensions.ParseLogData(input);

			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void ParseMisc()
		{
			var input = "t=2020-03-02T00:44:26-0600 lvl=info msg=\"no configuration paths supplied\"";
			var expectedResult = new Dictionary<string, string>()
			{
				{ "t", "2020-03-02T00:44:26-0600" },
				{ "lvl", "info" },
				{ "msg", "no configuration paths supplied" }
			};

			var result = NGrokLogExtensions.ParseLogData(input);

			Assert.Equal(expectedResult, result);
		}
	}
}
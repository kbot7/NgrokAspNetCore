using Ngrok.AspNetCore.Extensions;
using System.Collections.Generic;
using Xunit;

namespace Ngrok.AspNetCore.UnitTests
{
	public class NgrokLogExtensionsTests
	{
		[Fact]
		public void ParseTunnelStartLogDataIntoDictionary()
		{
			var logData = "t=2020-03-01T23:36:39-0600 lvl=info msg=\"started tunnel\" obj=tunnels name=test_tunnel addr=http://localhost:425 url=https://12030abc.ngrok.io";
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
			var result = NgrokLogExtensions.ParseLogData(logData);

			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void ParseNoConfigSuppliedLogDataIntoDictionary()
		{
			var logData = "t=2020-03-02T00:44:26-0600 lvl=info msg=\"no configuration paths supplied\"";
			var expectedResult = new Dictionary<string, string>()
			{
				{ "t", "2020-03-02T00:44:26-0600" },
				{ "lvl", "info" },
				{ "msg", "no configuration paths supplied" }
			};

			var result = NgrokLogExtensions.ParseLogData(logData);

			Assert.Equal(expectedResult, result);
		}
	}
}
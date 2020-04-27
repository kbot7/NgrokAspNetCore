using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ngrok.AspNetCore.Sample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ngrok.AspNetCore.Tests
{
	[TestClass]
	public class WebHostBuilderTest
	{
		private IHost _host;

		[TestMethod]
		public async Task CanCreateHostAndReachItViaNgrok()
		{
			_host = Program.CreateHostBuilder(Array.Empty<string>())
				.ConfigureWebHost(builder => builder
					.UseKestrel()
					.UseUrls("http://localhost:14568"))
				.Build();

			await _host.StartAsync();

			using var httpClient = new HttpClient();
			await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

			var ngrokService = _host.Services.GetRequiredService<INgrokHostedService>();
			var tunnels = await ngrokService.GetTunnelsAsync();

			Assert.AreEqual(2, tunnels.Count());

			foreach (var tunnel in tunnels)
				await AssertIsUrlReachableAsync(httpClient, tunnel.PublicURL);
		}

		private async Task AssertIsUrlReachableAsync(HttpClient httpClient, string url)
		{
			var stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
			{
				try
				{
					var response = await httpClient.GetAsync(url);
					response.EnsureSuccessStatusCode();

					await _host.StopAsync();
					return;
				}
				catch (HttpRequestException)
				{
				}

				await Task.Delay(1000);
			}

			await _host.StopAsync();
			Assert.Fail("Timeout for URL " + url);
		}
	}
}
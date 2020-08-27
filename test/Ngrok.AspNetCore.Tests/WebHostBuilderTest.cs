using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ngrok.AspNetCore.Sample;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore.Tests
{
	[TestClass]
	public class WebHostBuilderTest
	{
		[TestMethod]
		public async Task CanCreateHostAndReachItViaNgrok()
		{
			var host = Program.CreateHostBuilder(Array.Empty<string>())
				.ConfigureWebHost(builder => builder
					.UseKestrel()
					.UseUrls("http://*:14568"))
				.Build();

			await host.StartAsync();

			using var httpClient = new HttpClient();
			await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

			var ngrokService = host.Services.GetRequiredService<INgrokHostedService>();
			var tunnels = await ngrokService.GetTunnelsAsync();

			Assert.AreEqual(2, tunnels.Count());

			foreach (var tunnel in tunnels)
				await AssertIsUrlReachableAsync(httpClient, tunnel.PublicURL);
		}

		private static async Task AssertIsUrlReachableAsync(HttpClient httpClient, string url)
		{
			var stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
			{
				try
				{
					var response = await httpClient.GetAsync(url);
					response.EnsureSuccessStatusCode();

					return;
				}
				catch (HttpRequestException)
				{
				}

				await Task.Delay(1000);
			}

			Assert.Fail("Timeout for URL " + url);
		}
	}
}
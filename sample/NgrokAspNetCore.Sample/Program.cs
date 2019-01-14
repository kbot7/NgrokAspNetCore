using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NgrokAspNetCore.Lib;

namespace NgrokAspNetCore.Sample
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = CreateWebHostBuilder(args);

			var host = builder.Build();

			// Start Ngrok here
			await host.StartNgrokAsync();
	
			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}

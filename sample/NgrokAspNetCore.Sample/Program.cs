using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace NgrokAspNetCore.Sample
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = CreateWebHostBuilder(args);

			var host = builder.Build();

			// Start Ngrok here
			var tunnels = await host.StartNgrokAsync();

			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}

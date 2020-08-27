using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ngrok.AspNetCore.Services;
using System.Linq;
using Xunit;

namespace Ngrok.AspNetCore.UnitTests
{
	public class NgrokAspNetCoreAddNgrokAddsCorrectServicesTests
	{
		[Fact]
		public void AddNgrokAddsINgrokHostedServiceToServiceCollection()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok();
			var hasNgrokHostedService = services.Any(x => x.ServiceType == typeof(INgrokHostedService));
			Assert.True(hasNgrokHostedService);
		}

		[Fact]
		public void AddNgrokAddsNgrokProcessMgrToServiceCollection()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok();
			var hasNgrokProcessMgr= services.Any(x => x.ServiceType.Equals(typeof(NgrokProcessMgr)));
			Assert.True(hasNgrokProcessMgr);
		}

		[Fact]
		public void AddNgrokAddsNgokDownloaderToServiceCollection()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok();
			var hasNgrokDownloader = services.Any(x => x.ServiceType.Equals(typeof(NgrokDownloader)));
			Assert.True(hasNgrokDownloader);
		}

		[Fact]
		public void AddNgrokAddsNgokHostedServiceToServiceCollection()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok();
			var hasNgrokHostedService = services.Any(x => x.ServiceType.Equals(typeof(NgrokHostedService)));
			Assert.True(hasNgrokHostedService);
		}

		[Fact]
		public void AddNgrokAddsIHostedServiceToServiceCollection()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok();
			var hasHostedService = services.Any(x => x.ServiceType.Equals(typeof(IHostedService)));
			Assert.True(hasHostedService);
		}
	}
}

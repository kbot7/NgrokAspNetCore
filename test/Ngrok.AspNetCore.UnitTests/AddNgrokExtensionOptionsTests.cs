using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ngrok.AspNetCore.UnitTests
{
	public class AddNgrokExtensionOptionsTests
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, false)]
		[InlineData(true, true, true)]
		public void ManageNgrokProcessFalseSetsDownloadNgrok(bool manageNgrokProcess, bool downloadNgrok, bool expectedResult)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok(options =>
			{
				options.ManageNgrokProcess = manageNgrokProcess;
				options.DownloadNgrok = downloadNgrok;
			});

			var serviceProvider = services.BuildServiceProvider();

			var ngrokOptions = serviceProvider.GetService<IOptions<NgrokOptions>>().Value;
			Assert.Equal(ngrokOptions.DownloadNgrok, expectedResult);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, false)]
		[InlineData(true, true, true)]
		public void ManageNgrokProcessFalseSetsRedirectLogs(bool manageNgrokProcess, bool redirectLogs, bool expectedResult)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok(options =>
			{
				options.ManageNgrokProcess = manageNgrokProcess;
				options.RedirectLogs = redirectLogs;
			});

			var serviceProvider = services.BuildServiceProvider();

			var ngrokOptions = serviceProvider.GetService<IOptions<NgrokOptions>>().Value;
			Assert.Equal(ngrokOptions.RedirectLogs, expectedResult);
		}


		[Theory]
		[InlineData(false, null, null)]
		[InlineData(false, "Hello World", null)]
		[InlineData(true, null, null)]
		[InlineData(true, "Hello World", "Hello World")]
		public void ManageNgrokProcessFalseSetsNgrokConfigProfile(bool manageNgrokProcess, string ngrokConfigProfile, string expectedResult)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok(options =>
			{
				options.ManageNgrokProcess = manageNgrokProcess;
				options.NgrokConfigProfile = ngrokConfigProfile;
			});

			var serviceProvider = services.BuildServiceProvider();

			var ngrokOptions = serviceProvider.GetService<IOptions<NgrokOptions>>().Value;
			Assert.Equal(ngrokOptions.NgrokConfigProfile, expectedResult);
		}

		[Theory]
		[InlineData(false, 0, 0)]
		[InlineData(false, 10, 0)]
		[InlineData(true, 0, 0)]
		[InlineData(true, 10, 10)]
		public void ManageNgrokProcessFalseSetsProcessStartTimeoutMs(bool manageNgrokProcess, int processStartTimeoutMs, int expectedResult)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddNgrok(options =>
			{
				options.ManageNgrokProcess = manageNgrokProcess;
				options.ProcessStartTimeoutMs = processStartTimeoutMs;
			});

			var serviceProvider = services.BuildServiceProvider();
			var ngrokOptions = serviceProvider.GetService<IOptions<NgrokOptions>>().Value;
			Assert.Equal(ngrokOptions.ProcessStartTimeoutMs, expectedResult);
		}
	}
}

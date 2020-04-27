using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ngrok.ApiClient;
using Ngrok.AspNetCore.Sample.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore.Sample.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly INgrokHostedService _ngrokService;

		public HomeController(ILogger<HomeController> logger, INgrokHostedService ngrokService)
		{
			_logger = logger;
			_ngrokService = ngrokService;
		}

		public async Task<IActionResult> Index()
		{
			var tunnels = await _ngrokService.GetTunnelsAsync();
			tunnels = tunnels ?? new List<Tunnel>();
			return View(tunnels);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
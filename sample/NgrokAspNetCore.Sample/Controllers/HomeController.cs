using System.Diagnostics;
using System.Threading.Tasks;
using NgrokNgrokAspNetCore.Sample.Models;
using NgrokAspNetCore.Lib.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NgrokAspNetCore.Lib;

namespace NgrokNgrokAspNetCore.Sample.Controllers
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
            return View(tunnels);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

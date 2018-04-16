using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportsDirect.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace SportsDirect.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly OtherSettings _otherSettings;
        public HomeController(IHostingEnvironment hostingEnvironment, IOptions<OtherSettings> otherSettings)
        {
            _hostingEnvironment = hostingEnvironment;
            _otherSettings = otherSettings.Value;

        }

        [ActionName("Index")]
        public IActionResult Index()
        {
            ViewData["Location"] = _otherSettings.Location;
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

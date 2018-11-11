using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Polib.NetCore.Web.Models;

namespace Polib.NetCore.Web.Controllers
{
    public class HomeController : AppController
    {
        public HomeController(IConfiguration configuration, IOptions<TranslationSettings> translationSettings)
          : base(configuration, translationSettings)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = T("Your application description page.");

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = T("Your contact page.");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace FileDownloader.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(CancellationToken ct)
        {
            return View();
        }
    }
}
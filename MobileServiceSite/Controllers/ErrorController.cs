using Microsoft.AspNetCore.Mvc;

namespace MobileServiceSite.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace MobileServiceSite.Controllers
{
    public class SuccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

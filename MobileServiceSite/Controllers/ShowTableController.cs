using Microsoft.AspNetCore.Mvc;

namespace MobileServiceSite.Controllers
{
    public class ShowTableController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

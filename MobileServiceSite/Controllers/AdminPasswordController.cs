using Microsoft.AspNetCore.Mvc;

namespace MobileServiceSite.Controllers
{
    public class AdminPasswordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [HttpPost]
        public IActionResult Authenticate(string password)
        {
            if (password == "11081488")
            {
                return Redirect("/Admin");
            }
            else
            {
                return Redirect("/AdminPassword");
            }
        }
    }

}
    

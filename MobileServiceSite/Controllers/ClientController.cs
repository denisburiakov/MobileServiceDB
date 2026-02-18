using Microsoft.AspNetCore.Mvc;
using MobileServiceSite.Data;
using MobileServiceSite.Models;

namespace MobileServiceSite.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
       
        public async Task<IActionResult> Index(string first_name, string last_name, string phone, string email)
        {
            var lastId = _context.Clients.Any() ? _context.Clients.Max(c => c.Id) : 0;
            if (ModelState.IsValid)
            {
                var client = new Client
                {
                    Id = lastId + 1,
                    FirstName = first_name,
                    LastName = last_name,
                    Phone = phone,
                    Email = email,
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                return Redirect("/Success");
               
            }
            return View();

        }
    }
}
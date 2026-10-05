using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Страница регистрации клиента
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string first_name, string last_name, string phone, string email)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, есть ли такой клиент уже в базе (по номеру телефона)
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Phone == phone);

                if (existingClient != null)
                {
                    // Клиент уже существует — просто заходим в его личный кабинет
                    return RedirectToAction("Account", new { id = existingClient.Id });
                }

                // Клиента нет — регистрируем нового
                var lastId = _context.Clients.Any() ? _context.Clients.Max(c => c.Id) : 0;
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

                // Сразу в личный кабинет
                return RedirectToAction("Account", new { id = client.Id });
            }
            return View();
        }

        // Вход уже зарегистрированного клиента по номеру телефона
        [HttpGet]
        [Route("Client/Login")]
        public async Task<IActionResult> Login(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                TempData["ErrorMessage"] = "Введите номер телефона";
                return RedirectToAction("Index");
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Клиент с таким номером не найден. Зарегистрируйтесь.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Account", new { id = client.Id });
        }

        // Личный кабинет клиента
        public async Task<IActionResult> Account(int id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            var devices = await _context.Devices
                .Where(d => d.ClientId == id)
                .ToListAsync();

            var deviceIds = devices.Select(d => d.Id).ToList();

            var services = await _context.Services
                .Where(s => deviceIds.Contains(s.DeviceId))
                .Include(s => s.Category)
                .ToListAsync();

            var details = await _context.Details
                .Where(d => deviceIds.Contains(d.DeviceId))
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.Devices = devices;
            ViewBag.Services = services;
            ViewBag.Details = details;

            return View();
        }
    }
}

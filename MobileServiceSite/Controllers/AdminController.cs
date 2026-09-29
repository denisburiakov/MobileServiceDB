using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MobileServiceSite.Data;
using MobileServiceSite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;

namespace MobileServiceSite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Статистика для главной страницы
            ViewBag.ClientCount = _context.Clients.Count();
            ViewBag.DeviceCount = _context.Devices.Count();
            ViewBag.ServiceCount = _context.Services.Count();
            ViewBag.DetailCount = _context.Details.Count();
            ViewBag.CategoryCount = _context.Categories.Count();

            ViewBag.Tables = new List<string>
            {
                "Clients", "Devices", "Category", "Services", "Details"
            };

            ViewBag.Register = new List<string>{
                "Clients", "Devices", "Category", "Services", "Details"
            };

            return View();
        }

        // Методы для просмотра таблиц
        public async Task<IActionResult> ShowClients()
        {
            var clients = await _context.Clients.ToListAsync();
            ViewBag.TableName = "Clients";
            return View("ShowTable", clients);
        }

        public async Task<IActionResult> ShowDevices()
        {
            var devices = await _context.Devices
                .Include(d => d.Client)
                .ToListAsync();
            ViewBag.TableName = "Devices";
            return View("ShowTable", devices);
        }

        public async Task<IActionResult> ShowDetails()
        {
            var details = await _context.Details
                .Include(d => d.Device)
                .ToListAsync();
            ViewBag.TableName = "Details";
            return View("ShowTable", details);
        }

        public async Task<IActionResult> ShowServices()
        {
            var services = await _context.Services
                .Include(s => s.Device)
                .Include(s => s.Category)
                .ToListAsync();
            ViewBag.TableName = "Services";
            return View("ShowTable", services);
        }

        public async Task<IActionResult> ShowCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.TableName = "Categories";
            return View("ShowTable", categories);
        }

        [HttpGet]
        [Route("ShowTable")]
        public async Task<IActionResult> ShowTable(string tableName)
        {
            object data = null;

            switch (tableName.ToLower())
            {
                case "clients":
                    data = await _context.Clients.ToListAsync();
                    break;
                case "devices":
                    data = await _context.Devices
                        .Include(d => d.Client)
                        .ToListAsync();
                    break;
                case "details":
                    data = await _context.Details
                        .Include(d => d.Device)
                        .ToListAsync();
                    break;
                case "services":
                    data = await _context.Services
                        .Include(s => s.Device)
                        .Include(s => s.Category)
                        .ToListAsync();
                    break;
                case "category":
                    data = await _context.Categories.ToListAsync();
                    break;
                default:
                    return NotFound();
            }

            ViewBag.TableName = tableName;
            return View("ShowTable", data);
        }

        // GET методы для регистрации
        [HttpGet]
        [Route("Register")]
        public IActionResult Register(string registerName)
        {
            switch (registerName.ToLower())
            {
                case "clients":
                    return View("RegisterClient");
                case "devices":
                    return View("RegisterDevice");
                case "details":
                    return View("RegisterDetail");
                case "services":
                    return View("RegisterService");
                case "category":
                    return View("RegisterCategory");
                default:
                    return RedirectToAction("Index");
            }
        }

        // POST методы для регистрации
        [HttpPost]
       
        public async Task<IActionResult> RegisterClient(string first_name, string last_name, string phone, string email)
        {
            var lastId = _context.Clients.Any() ? _context.Clients.Max(c => c.Id) : 0;
            ModelState.Clear();
            if (ModelState.IsValid) { 
                
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


        [HttpPost]
        public async Task<IActionResult> RegisterDevice(int client_id, string type_of_device, string producer, string model, string serial_number, string def_descriotion)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                var lastId = _context.Devices.Any() ? _context.Devices.Max(d => d.Id) : 0;
                var device = new Device { 
                    Id = lastId + 1,
                    ClientId = client_id,
                    TypeOfDevice = type_of_device,
                    Producer = producer,    
                    Model = model,
                    SerialNumber = serial_number,
                    DefDescriotion = def_descriotion
                };
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                return Redirect("/Success");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDetail(int device_id, string type_of_detail, int cost_of_detail, string producer_det)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                var lastId = _context.Details.Any() ? _context.Details.Max(d => d.Id) : 0;
                var detail = new Detail
                {
                    Id = lastId + 1,
                    DeviceId = device_id,
                    TypeOfDetail = type_of_detail,
                    CostOfDetail = cost_of_detail,
                    ProducerDet = producer_det 
                };
                _context.Details.Add(detail);
                await _context.SaveChangesAsync();

                return Redirect("/Success");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterService(int device_id, int category_id, string type_of_service, string time_of_doing, int cost_of_service)
        {

            ModelState.Clear();
            if (ModelState.IsValid)
            { 

                var lastId = _context.Services.Any() ? _context.Services.Max(s => s.Id) : 0;
                var service = new Service
                {
                    Id = lastId + 1,
                    DeviceId = device_id,
                    CategoryId = category_id,
                    TimeOfDoing = time_of_doing,
                    TypeOfService = type_of_service,
                    CostOfService = cost_of_service
                };
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return Redirect("/Success");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCategory(string category)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                
                var lastId = _context.Categories.Any() ? _context.Categories.Max(c => c.Id) : 0;
                var categories = new Category
                {
                    Id = lastId + 1,
                    Categories = category
                };
                
                _context.Categories.Add(categories);
                await _context.SaveChangesAsync();

                return Redirect("/Success");
            }
            return View();
        }

        [HttpGet]
        [Route("ShowRegister")]
        public IActionResult ShowRegister(string registerName)
        {
            return RedirectToAction("Register", new { registerName });
        }

        // Метод поиска клиента по телефону
        [HttpGet]
        [Route("Admin/SearchClient")]
        public async Task<IActionResult> SearchClient(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                TempData["ErrorMessage"] = "Please enter phone number";
                return RedirectToAction("Index");
            }

            // Ищем клиента по номеру телефона
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone.Contains(phone));

            if (client == null)
            {
                TempData["ErrorMessage"] = $"Client with phone '{phone}' not found";
                return Redirect("/Error");
            }

            // Получаем все устройства клиента
            var devices = await _context.Devices
                .Where(d => d.ClientId == client.Id)
                .ToListAsync();

            // Получаем все сервисы для этих устройств
            var deviceIds = devices.Select(d => d.Id).ToList();
            var services = await _context.Services
                .Where(s => deviceIds.Contains(s.DeviceId))
                .Include(s => s.Category)
                .ToListAsync();

            // Получаем все детали для этих устройств
            var details = await _context.Details
                .Where(d => deviceIds.Contains(d.DeviceId))
                .ToListAsync();

            // Собираем все в ViewBag
            ViewBag.Client = client;
            ViewBag.Devices = devices;
            ViewBag.Services = services;
            ViewBag.Details = details;
            ViewBag.SearchPhone = phone;

            return View("ClientInfo");
        }
    }
}


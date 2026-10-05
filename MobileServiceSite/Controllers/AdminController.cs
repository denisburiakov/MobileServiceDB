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

            // Сколько заказов оплачено и ждут, когда админ приступит к работе
            ViewBag.NewPaymentCount = _context.Orders.Count(o => o.Status == OrderStatus.Paid);

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
                TempData["ErrorMessage"] = "Введите номер телефона";
                return RedirectToAction("Index");
            }

            // Ищем клиента по номеру телефона
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone.Contains(phone));

            if (client == null)
            {
                TempData["ErrorMessage"] = $"Клиент с телефоном '{phone}' не найден";
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

        // ================= ЗАКАЗЫ =================

        // Список заказов: новые, ждущие цены, оплаченные, в работе
        [HttpGet]
        [Route("Admin/Orders")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Device)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Уведомления об оплате: заказ оплачен, но работа ещё не начата
            ViewBag.NewPayments = orders
                .Where(o => o.Status == OrderStatus.Paid)
                .ToList();
            ViewBag.WaitingPriceCount = orders.Count(o => o.Status == OrderStatus.Created);
            ViewBag.AwaitingPaymentCount = orders.Count(o => o.Status == OrderStatus.AwaitingPayment);
            ViewBag.InWorkCount = orders.Count(o => o.Status == OrderStatus.InProgress);
            ViewBag.CompletedCount = orders.Count(o => o.Status == OrderStatus.Completed);

            return View(orders);
        }

        // Админ устанавливает цену по заказу
        [HttpPost]
        [Route("Admin/SetOrderPrice")]
        public async Task<IActionResult> SetOrderPrice(int order_id, int price)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == order_id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Created || order.Price != null)
            {
                TempData["ErrorMessage"] = $"По заявке №{order.Id} цена уже установлена";
                return RedirectToAction("Orders");
            }

            if (price <= 0)
            {
                TempData["ErrorMessage"] = "Цена должна быть больше нуля";
                return RedirectToAction("Orders");
            }

            order.Price = price;
            order.Status = OrderStatus.AwaitingPayment;
            order.PriceSetAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Заявка №{order.Id}: цена {price} BYN установлена. Клиент получил возможность оплатить заказ.";
            return RedirectToAction("Orders");
        }

        // Админ приступает к работе после получения подтверждения об оплате
        [HttpPost]
        [Route("Admin/StartWork")]
        public async Task<IActionResult> StartWork(int order_id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == order_id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Paid)
            {
                TempData["ErrorMessage"] =
                    $"Заказ №{order.Id} нельзя взять в работу: нет подтверждения об оплате";
                return RedirectToAction("Orders");
            }

            order.Status = OrderStatus.InProgress;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заказ №{order.Id} оплачен — работа начата";
            return RedirectToAction("Orders");
        }

        // Заказ выполнен
        [HttpPost]
        [Route("Admin/CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int order_id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == order_id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.InProgress)
            {
                TempData["ErrorMessage"] = $"Заказ №{order.Id} не находится в работе";
                return RedirectToAction("Orders");
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заказ №{order.Id} выполнен";
            return RedirectToAction("Orders");
        }

        // ================= РЕДАКТИРОВАНИЕ =================

        /// <summary>
        /// Админ правит заявку клиента: данные устройства и описание работы.
        /// Заявку составляет клиент, админ только корректирует, если что-то указано неверно.
        /// </summary>
        [HttpGet]
        [Route("Admin/EditOrder/{id:int}")]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Device)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [Route("Admin/EditOrder/{id:int}")]
        public async Task<IActionResult> EditOrder(
            int id,
            string type_of_device,
            string producer,
            string model,
            string serial_number,
            string def_descriotion,
            string description)
        {
            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Device)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            string? error = ValidateDeviceAndDescription(
                type_of_device, producer, model, serial_number, def_descriotion, description, out var type, out var prod,
                out var mod, out var serial, out var problem, out var descr);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction("EditOrder", new { id });
            }

            order.Device.TypeOfDevice = type!;
            order.Device.Producer = prod!;
            order.Device.Model = mod!;
            order.Device.SerialNumber = serial!;
            order.Device.DefDescriotion = problem!;
            order.Description = descr!;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка №{order.Id} обновлена";
            return RedirectToAction("Orders");
        }

        /// <summary>Админ правит данные устройства клиента</summary>
        [HttpGet]
        [Route("Admin/EditDevice/{id:int}")]
        public async Task<IActionResult> EditDevice(int id)
        {
            var device = await _context.Devices
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        [HttpPost]
        [Route("Admin/EditDevice/{id:int}")]
        public async Task<IActionResult> EditDevice(
            int id,
            string type_of_device,
            string producer,
            string model,
            string serial_number,
            string def_descriotion)
        {
            var device = await _context.Devices
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            string? error = ValidateDeviceAndDescription(
                type_of_device, producer, model, serial_number, def_descriotion, "описание",
                out var type, out var prod, out var mod, out var serial, out var problem, out _);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction("EditDevice", new { id });
            }

            device.TypeOfDevice = type!;
            device.Producer = prod!;
            device.Model = mod!;
            device.SerialNumber = serial!;
            device.DefDescriotion = problem!;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Данные устройства обновлены";
            return RedirectToAction("SearchClient", new { phone = device.Client.Phone });
        }

        /// <summary>
        /// Общая проверка данных устройства и описания работы.
        /// Возвращает текст ошибки или null, если всё в порядке.
        /// </summary>
        private static string? ValidateDeviceAndDescription(
            string? typeOfDevice,
            string? producer,
            string? model,
            string? serialNumber,
            string? defDescription,
            string? description,
            out string? type,
            out string? prod,
            out string? mod,
            out string? serial,
            out string? problem,
            out string? descr)
        {
            type = (typeOfDevice ?? string.Empty).Trim();
            prod = (producer ?? string.Empty).Trim();
            mod = (model ?? string.Empty).Trim();
            serial = (serialNumber ?? string.Empty).Trim();
            problem = (defDescription ?? string.Empty).Trim();
            descr = (description ?? string.Empty).Trim();

            if (type.Length == 0 || prod.Length == 0 || mod.Length == 0)
            {
                return "Заполните тип устройства, производителя и модель";
            }

            if (type.Length > 50 || prod.Length > 50 || mod.Length > 50 ||
                serial.Length > 50 || problem.Length > 50)
            {
                return "Слишком длинные данные устройства — максимум 50 символов в поле";
            }

            if (descr.Length == 0 || descr.Length > 500)
            {
                return "Описание заявки должно быть от 1 до 500 символов";
            }

            if (serial.Length == 0)
            {
                serial = "не указан";
            }

            return null;
        }
    }
}


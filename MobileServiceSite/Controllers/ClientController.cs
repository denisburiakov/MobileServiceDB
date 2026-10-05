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

            var orders = await _context.Orders
                .Where(o => o.ClientId == id)
                .Include(o => o.Device)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.Devices = devices;
            ViewBag.Services = services;
            ViewBag.Details = details;
            ViewBag.Orders = orders;

            return View();
        }

        // Создание заказа из личного кабинета
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int client_id, int device_id, string description)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == client_id);
            if (client == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == device_id && d.ClientId == client_id);

            if (device == null)
            {
                TempData["ErrorMessage"] = "Устройство не найдено";
                return RedirectToAction("Account", new { id = client_id });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Опишите, что нужно сделать с устройством";
                return RedirectToAction("Account", new { id = client_id });
            }

            var lastId = _context.Orders.Any() ? _context.Orders.Max(o => o.Id) : 0;
            var order = new Order
            {
                Id = lastId + 1,
                ClientId = client_id,
                DeviceId = device_id,
                Description = description.Trim(),
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заказ создан. Админ установит цену — после этого вы сможете его оплатить.";
            return RedirectToAction("Account", new { id = client_id });
        }

        /// <summary>
        /// Клиент сам составляет заявку на ремонт:
        /// выбирает своё устройство или добавляет новое (тип, производитель,
        /// модель, серийный номер, описание неисправности) и описывает работу.
        /// Админ в дальнейшем только правит данные при необходимости.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRequest(
            int client_id,
            int device_id,
            string? device_type,
            string? producer,
            string? model,
            string? serial_number,
            string? device_problem,
            string? description)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == client_id);
            if (client == null)
            {
                return NotFound();
            }

            description = (description ?? string.Empty).Trim();

            if (description.Length == 0)
            {
                TempData["ErrorMessage"] = "Опишите, что нужно сделать с устройством";
                return RedirectToAction("Account", new { id = client_id });
            }

            if (description.Length > 500)
            {
                TempData["ErrorMessage"] = "Описание заявки слишком длинное — максимум 500 символов";
                return RedirectToAction("Account", new { id = client_id });
            }

            Device device;

            if (device_id > 0)
            {
                // Клиент выбрал уже добавленное устройство
                device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.Id == device_id && d.ClientId == client_id);

                if (device == null)
                {
                    TempData["ErrorMessage"] = "Устройство не найдено. Выберите устройство из списка или добавьте новое.";
                    return RedirectToAction("Account", new { id = client_id });
                }
            }
            else
            {
                // Клиент добавляет новое устройство — данные заполняет он сам
                device_type = (device_type ?? string.Empty).Trim();
                producer = (producer ?? string.Empty).Trim();
                model = (model ?? string.Empty).Trim();

                if (device_type.Length == 0 || producer.Length == 0 || model.Length == 0)
                {
                    TempData["ErrorMessage"] = "Заполните тип устройства, производителя и модель";
                    return RedirectToAction("Account", new { id = client_id });
                }

                if (device_type.Length > 50 || producer.Length > 50 || model.Length > 50)
                {
                    TempData["ErrorMessage"] = "Слишком длинные данные устройства — максимум 50 символов в поле";
                    return RedirectToAction("Account", new { id = client_id });
                }

                serial_number = (serial_number ?? string.Empty).Trim();
                if (serial_number.Length > 50)
                {
                    TempData["ErrorMessage"] = "Серийный номер слишком длинный — максимум 50 символов";
                    return RedirectToAction("Account", new { id = client_id });
                }

                device_problem = (device_problem ?? string.Empty).Trim();
                if (device_problem.Length > 50)
                {
                    TempData["ErrorMessage"] = "Описание неисправности слишком длинное — максимум 50 символов";
                    return RedirectToAction("Account", new { id = client_id });
                }

                // Поля, которые клиент заполнять не обязан, подставляем сами
                if (serial_number.Length == 0)
                {
                    serial_number = "не указан";
                }

                if (device_problem.Length == 0)
                {
                    device_problem = description.Length > 50
                        ? description[..50]
                        : description;
                }

                var lastDeviceId = _context.Devices.Any() ? _context.Devices.Max(d => d.Id) : 0;
                device = new Device
                {
                    Id = lastDeviceId + 1,
                    ClientId = client_id,
                    TypeOfDevice = device_type,
                    Producer = producer,
                    Model = model,
                    SerialNumber = serial_number,
                    DefDescriotion = device_problem
                };
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }

            var lastOrderId = _context.Orders.Any() ? _context.Orders.Max(o => o.Id) : 0;
            var order = new Order
            {
                Id = lastOrderId + 1,
                ClientId = client_id,
                DeviceId = device.Id,
                Description = description,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Заявка №{order.Id} создана. Администратор посмотрит её и установит цену — после этого вы сможете оплатить.";
            return RedirectToAction("Account", new { id = client_id });
        }

        // Страница оплаты заказа
        [HttpGet]
        [Route("Client/Pay/{id:int}")]
        public async Task<IActionResult> Pay(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Device)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Оплатить можно только заказ, по которому админ уже установил цену
            if (order.Status == OrderStatus.Paid)
            {
                TempData["ErrorMessage"] = "Этот заказ уже оплачен";
                return RedirectToAction("Account", new { id = order.ClientId });
            }

            if (order.Status != OrderStatus.AwaitingPayment || order.Price == null)
            {
                TempData["ErrorMessage"] = "Оплата недоступна: админ ещё не установил цену по этому заказу";
                return RedirectToAction("Account", new { id = order.ClientId });
            }

            return View(order);
        }

        // Подтверждение оплаты (имитация платежа)
        [HttpPost]
        [Route("Client/ConfirmPayment")]
        public async Task<IActionResult> ConfirmPayment(int order_id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == order_id);

            if (order == null)
            {
                return NotFound();
            }

            // Повторная проверка условий оплаты
            if (order.Status == OrderStatus.Paid)
            {
                TempData["ErrorMessage"] = "Этот заказ уже оплачен";
                return RedirectToAction("Account", new { id = order.ClientId });
            }

            if (order.Status != OrderStatus.AwaitingPayment || order.Price == null)
            {
                TempData["ErrorMessage"] = "Оплата недоступна: админ ещё не установил цену по этому заказу";
                return RedirectToAction("Account", new { id = order.ClientId });
            }

            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Заказ №{order.Id} оплачен ({order.Price} BYN). Админ получил уведомление и может приступать к работе.";

            return RedirectToAction("Account", new { id = order.ClientId });
        }
    }
}

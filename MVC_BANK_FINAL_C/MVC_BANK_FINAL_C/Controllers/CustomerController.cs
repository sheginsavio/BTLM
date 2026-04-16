using Microsoft.AspNetCore.Mvc;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        private string Role => HttpContext.Session.GetString("UserRole") ?? "";
        private int? SessionCustomerId
        {
            get
            {
                var s = HttpContext.Session.GetString("CustomerId");
                return int.TryParse(s, out int id) ? id : null;
            }
        }

        // Admin, Teller
        public async Task<IActionResult> Index()
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var customers = await _customerService.GetAllCustomers();
            return View(customers);
        }

        // Admin, Teller
        public IActionResult Create()
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel vm)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid) return View(vm);

            await _customerService.CreateAccount(vm);
            TempData["Success"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Admin, Teller
        public async Task<IActionResult> Edit(int id)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var customer = await _customerService.GetAccountDetails(id);
            if (customer == null) return NotFound();

            var vm = new CustomerViewModel
            {
                CustomerId  = customer.CustomerId,
                Name        = customer.Name,
                Email       = customer.Email,
                ContactInfo = customer.ContactInfo
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel vm)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid) return View(vm);

            var result = await _customerService.UpdateCustomerInfo(id, vm);
            if (result == null) return NotFound();

            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Admin, Teller → any; Customer → own profile only
        public async Task<IActionResult> Details(int id)
        {
            if (Role == "Customer")
            {
                if (SessionCustomerId != id)
                {
                    TempData["Error"] = "Access Denied.";
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var customer = await _customerService.GetAccountDetails(id);
            if (customer == null) return NotFound();

            ViewBag.CanCreateAccount = (Role == "Admin" || Role == "Teller");
            return View(customer);
        }

        // ── AJAX: Customer search for Admin/Teller ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string term)
        {
            if (Role != "Admin" && Role != "Teller")
                return Json(new { success = false, message = "Access denied" });

            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var all = await _customerService.GetAllCustomers();
            var results = all
                .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                         || c.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(c => new { customerId = c.CustomerId, name = c.Name, email = c.Email })
                .ToList();

            return Json(results);
        }
    }
}

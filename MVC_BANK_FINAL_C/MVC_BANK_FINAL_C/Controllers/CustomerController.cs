using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Helpers;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly BankingDbContext _context;

        public CustomerController(ICustomerService customerService, BankingDbContext context)
        {
            _customerService = customerService;
            _context         = context;
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

            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == vm.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "A customer with this email already exists.");
                return View(vm);
            }

            var mobileExists = await _context.Customers
                .AnyAsync(c => c.ContactInfo == vm.ContactInfo);
            if (mobileExists)
            {
                ModelState.AddModelError("ContactInfo",
                    "A customer with this phone number already exists.");
                return View(vm);
            }

            if (!ModelState.IsValid) return View(vm);

            var customer = await _customerService.CreateAccount(vm);

            // Auto-generate username from name (lowercase, no spaces) + customerId
            string baseUsername    = vm.Name.ToLower().Replace(" ", "") + customer.CustomerId;
            string defaultPassword = "Bank@" + customer.CustomerId;

            // Check if username already exists, append number if needed
            string username = baseUsername;
            int suffix = 1;
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = baseUsername + suffix++;
            }

            var user = new User
            {
                Username     = username,
                Password     = PasswordHelper.HashPassword(defaultPassword),
                Role         = "Customer",
                CustomerId   = customer.CustomerId,
                IsFirstLogin = true   // Force password change on first login
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Customer created. Login: Username = '{username}', Default Password = 'Bank@{customer.CustomerId}'. Please share with the customer.";
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
            ViewBag.IsAdmin          = (Role == "Admin");
            return View(customer);
        }

        // AJAX: Customer search for Admin/Teller 
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

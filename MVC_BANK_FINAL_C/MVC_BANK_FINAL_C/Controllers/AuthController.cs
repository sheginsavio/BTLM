using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class AuthController : Controller
    {
        private readonly BankingDbContext _context;

        public AuthController(BankingDbContext context)
        {
            _context = context;
        }

        // ── Login ────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Login()
        {
            // Already logged in → go to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == vm.Username && u.Password == vm.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(vm);
            }

            // Store session
            HttpContext.Session.SetString("UserId",   user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.CustomerId.HasValue)
                HttpContext.Session.SetString("CustomerId", user.CustomerId.Value.ToString());

            return RedirectToAction("Index", "Home");
        }

        // ── Register ─────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Check duplicate username
            bool usernameTaken = await _context.Users.AnyAsync(u => u.Username == vm.Username);
            if (usernameTaken)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(vm);
            }

            // Create Customer
            var customer = new Customer
            {
                Name        = vm.Name,
                Email       = vm.Email,
                ContactInfo = vm.ContactInfo
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Create User linked to customer
            var user = new User
            {
                Username   = vm.Username,
                Password   = vm.Password,
                Role       = "Customer",
                CustomerId = customer.CustomerId
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // ── Logout ───────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(LogoutSuccess));
        }

        [AllowAnonymous]
        public IActionResult LogoutSuccess()
        {
            return View();
        }
    }
}

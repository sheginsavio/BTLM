using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankingDbContext _context;

        public AccountController(BankingDbContext context)
        {
            _context = context;
        }

        private string Role => HttpContext.Session.GetString("UserRole") ?? "";

        // GET: Account/Create?customerId=X
        // Admin, Teller only
        public async Task<IActionResult> Create(int customerId)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound();

            var vm = new AccountViewModel
            {
                CustomerId   = customerId,
                CustomerName = customer.Name
            };
            return View(vm);
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountViewModel vm)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // CustomerName is display-only — not required for model validity
            ModelState.Remove("CustomerName");

            if (!ModelState.IsValid)
            {
                var c = await _context.Customers.FindAsync(vm.CustomerId);
                vm.CustomerName = c?.Name ?? "";
                return View(vm);
            }

            var account = new Account
            {
                CustomerId  = vm.CustomerId,
                AccountType = vm.AccountType,
                Balance     = vm.InitialBalance
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Account created successfully for {vm.CustomerName}.";
            return RedirectToAction("Details", "Customer", new { id = vm.CustomerId });
        }
    }
}

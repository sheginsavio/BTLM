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

        // ── Account Deletion (Admin only) ─────────────────────────

        // GET: Account/CanDelete?accountId=X — safety check returning JSON
        [HttpGet]
        public async Task<IActionResult> CanDelete(int accountId)
        {
            if (Role != "Admin")
                return Json(new { canDelete = false, reason = "Access denied." });

            var account = await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                return Json(new { canDelete = false, reason = "Account not found." });

            // Cannot delete if balance > 0
            if (account.Balance > 0)
                return Json(new
                {
                    canDelete = false,
                    reason = $"Cannot delete account with balance of " +
                             $"{account.Balance.ToString("C", new System.Globalization.CultureInfo("en-IN"))}. " +
                             $"Please withdraw all funds first."
                });

            // Cannot delete if has active/approved loan
            bool hasActiveLoan = await _context.Loans
                .AnyAsync(l => l.CreditAccountId == accountId
                            && l.LoanStatus == LoanStatus.APPROVED);
            if (hasActiveLoan)
                return Json(new
                {
                    canDelete = false,
                    reason = "Cannot delete account with an active loan linked to it."
                });

            return Json(new { canDelete = true, reason = "" });
        }

        // POST: Account/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int accountId, int customerId)
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Details", "Customer", new { id = customerId });
            }

            // Safety checks before delete
            if (account.Balance > 0)
            {
                TempData["Error"] =
                    $"Cannot delete account with remaining balance. " +
                    $"Please withdraw all funds first.";
                return RedirectToAction("Details", "Customer", new { id = customerId });
            }

            bool hasActiveLoan = await _context.Loans
                .AnyAsync(l => l.CreditAccountId == accountId
                            && l.LoanStatus == LoanStatus.APPROVED);
            if (hasActiveLoan)
            {
                TempData["Error"] =
                    "Cannot delete account with an active loan linked to it.";
                return RedirectToAction("Details", "Customer", new { id = customerId });
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Account #{accountId} has been permanently deleted.";
            return RedirectToAction("Details", "Customer", new { id = customerId });
        }
    }
}

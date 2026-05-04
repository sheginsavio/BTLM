using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly BankingDbContext _context;

        public TransactionController(ITransactionService transactionService, BankingDbContext context)
        {
            _transactionService = transactionService;
            _context            = context;
        }

        private string Role => HttpContext.Session.GetString("UserRole") ?? "";
        private string SessionUsername => HttpContext.Session.GetString("Username") ?? "System";
        private int? SessionCustomerId
        {
            get
            {
                var s = HttpContext.Session.GetString("CustomerId");
                return int.TryParse(s, out int id) ? id : null;
            }
        }

        // ── Helper: build SelectListItem list for a customer's accounts ──────────
        private async Task<List<SelectListItem>> BuildAccountList(int customerId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            return accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text  = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
            }).ToList();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  INDEX  – Admin, Teller, Auditor, Customer (own only)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            if (Role != "Admin" && Role != "Teller" && Role != "Auditor" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var all = await _transactionService.GetAllTransactions();

            if (Role == "Customer")
            {
                int cid = SessionCustomerId ?? 0;
                all = all.Where(t => t.Account?.CustomerId == cid);
            }

            ViewBag.UserRole = Role;
            return View(all);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  JSON API ENDPOINTS
        // ════════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetCustomerAccounts(int customerId)
        {
            if (Role != "Admin" && Role != "Teller" && Role != "LoanOfficer")
                return Json(new { success = false, message = "Access denied" });

            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            var list = accounts.Select(a => new
            {
                accountId = a.AccountId,
                label     = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
            });

            return Json(new { success = true, accounts = list });
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAccount(int accountId)
        {
            if (Role != "Admin" && Role != "Teller" && Role != "Customer")
                return Json(new { success = false, message = "Access denied" });

            var account = await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                return Json(new { success = false, message = "Account not found" });

            return Json(new
            {
                success       = true,
                accountHolder = account.Customer?.Name ?? "Unknown",
                accountType   = account.AccountType.ToString(),
                balance       = account.Balance
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            if (Role != "Customer")
                return Json(new { success = false, message = "Access denied" });

            int cid      = SessionCustomerId ?? 0;
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == cid)
                .ToListAsync();

            var list = accounts.Select(a => new
            {
                accountId = a.AccountId,
                label     = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
            });

            return Json(list);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  DEPOSIT
        // ════════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Deposit()
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var vm = new TransactionViewModel { TransactionType = Data.TransactionType.DEPOSIT };

            ViewBag.ShowCustomerSearch = true;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(TransactionViewModel vm)
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            vm.TransactionType = Data.TransactionType.DEPOSIT;
            vm.PerformedBy     = SessionUsername;

            // Customer security: verify the account belongs to them
            if (Role == "Customer")
            {
                int cid     = SessionCustomerId ?? 0;
                var account = await _context.Accounts.FindAsync(vm.AccountId);
                if (account == null || account.CustomerId != cid)
                {
                    ModelState.AddModelError("", "Invalid account selection.");
                    vm.FromAccountList         = await BuildAccountList(cid);
                    ViewBag.ShowCustomerSearch = false;
                    return View(vm);
                }
            }

            ModelState.Remove("TransferType");
            ModelState.Remove("PerformedBy");
            if (!ModelState.IsValid)
            {
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer") vm.FromAccountList = await BuildAccountList(SessionCustomerId ?? 0);
                return View(vm);
            }

            var result = await _transactionService.DepositFunds(vm);
            if (result == null)
            {
                ModelState.AddModelError("", "Deposit failed. Account not found.");
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer") vm.FromAccountList = await BuildAccountList(SessionCustomerId ?? 0);
                return View(vm);
            }

            TempData["Success"] = $"Successfully deposited {vm.Amount:C} to account {vm.AccountId}.";
            return RedirectToAction(nameof(Index));
        }

        // ════════════════════════════════════════════════════════════════════════
        //  WITHDRAW
        // ════════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Withdraw()
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var vm = new TransactionViewModel { TransactionType = Data.TransactionType.WITHDRAWAL };

            if (Role == "Customer")
            {
                int cid            = SessionCustomerId ?? 0;
                vm.FromAccountList = await BuildAccountList(cid);
                ViewBag.ShowCustomerSearch = false;
            }
            else
            {
                ViewBag.ShowCustomerSearch = true;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(TransactionViewModel vm)
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            vm.TransactionType = Data.TransactionType.WITHDRAWAL;
            vm.PerformedBy     = SessionUsername;

            if (Role == "Customer")
            {
                int cid     = SessionCustomerId ?? 0;
                var account = await _context.Accounts.FindAsync(vm.AccountId);
                if (account == null || account.CustomerId != cid)
                {
                    ModelState.AddModelError("", "Invalid account selection.");
                    vm.FromAccountList         = await BuildAccountList(cid);
                    ViewBag.ShowCustomerSearch = false;
                    return View(vm);
                }
            }

            ModelState.Remove("TransferType");
            ModelState.Remove("PerformedBy");
            if (!ModelState.IsValid)
            {
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer") vm.FromAccountList = await BuildAccountList(SessionCustomerId ?? 0);
                return View(vm);
            }

            var result = await _transactionService.WithdrawFunds(vm);
            if (result == null)
            {
                ModelState.AddModelError("", "Withdrawal failed. Check account or insufficient balance.");
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer") vm.FromAccountList = await BuildAccountList(SessionCustomerId ?? 0);
                return View(vm);
            }

            TempData["Success"] = $"Successfully withdrew {vm.Amount:C} from account {vm.AccountId}.";
            return RedirectToAction(nameof(Index));
        }

        // ════════════════════════════════════════════════════════════════════════
        //  TRANSFER
        // ════════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Transfer()
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var vm = new TransactionViewModel { TransactionType = Data.TransactionType.TRANSFER };

            if (Role == "Customer")
            {
                int cid            = SessionCustomerId ?? 0;
                var list           = await BuildAccountList(cid);
                vm.FromAccountList = list;
                vm.ToAccountList   = list;
                ViewBag.ShowCustomerSearch = false;
            }
            else
            {
                ViewBag.ShowCustomerSearch = true;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransactionViewModel vm)
        {
            if (Role == "LoanOfficer" || Role == "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (Role != "Admin" && Role != "Teller" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            vm.TransactionType = Data.TransactionType.TRANSFER;
            vm.PerformedBy     = SessionUsername;

            ModelState.Remove("TransferType");
            ModelState.Remove("PerformedBy");

            // Basic validation
            if (vm.AccountId <= 0)
            {
                ModelState.AddModelError("", "Please select a valid source account.");
            }
            if (vm.ToAccountId == null || vm.ToAccountId <= 0)
            {
                ModelState.AddModelError("", "Please select or verify a destination account.");
            }
            if (vm.AccountId == vm.ToAccountId)
            {
                ModelState.AddModelError("", "Source and destination accounts cannot be the same.");
            }

            // Customer ownership validation
            if (Role == "Customer" && ModelState.ErrorCount == 0)
            {
                int cid = SessionCustomerId ?? 0;

                var fromAccount = await _context.Accounts.FindAsync(vm.AccountId);
                if (fromAccount == null || fromAccount.CustomerId != cid)
                {
                    ModelState.AddModelError("", "Invalid source account.");
                }

                // For own-to-own transfer, also validate destination ownership
                if (vm.TransferType == "own" && ModelState.ErrorCount == 0)
                {
                    var toAccount = await _context.Accounts.FindAsync(vm.ToAccountId!.Value);
                    if (toAccount == null || toAccount.CustomerId != cid)
                    {
                        ModelState.AddModelError("", "Invalid destination account for own-account transfer.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer")
                {
                    int cid            = SessionCustomerId ?? 0;
                    var list           = await BuildAccountList(cid);
                    vm.FromAccountList = list;
                    vm.ToAccountList   = list;
                }
                return View(vm);
            }

            var result = await _transactionService.TransferFunds(
                vm.AccountId,
                vm.ToAccountId!.Value,
                vm.Amount,
                vm.PerformedBy ?? "System");

            if (!result)
            {
                ModelState.AddModelError("", "Transfer failed. Check account IDs or insufficient balance.");
                ViewBag.ShowCustomerSearch = Role != "Customer";
                if (Role == "Customer")
                {
                    int cid            = SessionCustomerId ?? 0;
                    var list           = await BuildAccountList(cid);
                    vm.FromAccountList = list;
                    vm.ToAccountList   = list;
                }
                return View(vm);
            }

            TempData["Success"] = $"Successfully transferred {vm.Amount:C} from account {vm.AccountId} to account {vm.ToAccountId}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

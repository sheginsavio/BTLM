using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class RepaymentController : Controller
    {
        private readonly IRepaymentService _repaymentService;
        private readonly ILoanService      _loanService;
        private readonly BankingDbContext  _context;

        public RepaymentController(
            IRepaymentService repaymentService,
            ILoanService loanService,
            BankingDbContext context)
        {
            _repaymentService = repaymentService;
            _loanService      = loanService;
            _context          = context;
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

        // ════════════════════════════════════════════════════════════════════════
        //  INDEX — Admin, LoanOfficer, Auditor, Customer (own only)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(int loanId)
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Auditor" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var loan = await _loanService.GetLoanDetails(loanId);

            // Customer: verify they own the loan
            if (Role == "Customer" && (loan == null || loan.CustomerId != SessionCustomerId))
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var repayments = await _repaymentService.GetRepaymentHistory(loanId);
            ViewBag.LoanId = loanId;
            ViewBag.Loan   = loan;
            return View(repayments);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  RECORD — Admin, LoanOfficer
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Record(int? loanId)
        {
            if (Role != "Admin" && Role != "LoanOfficer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var loans = await _loanService.GetAllLoans();
            ViewBag.Loans = loans;
            var vm = new RepaymentViewModel();
            if (loanId.HasValue) vm.LoanId = loanId.Value;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(RepaymentViewModel vm)
        {
            if (Role != "Admin" && Role != "LoanOfficer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Loans = await _loanService.GetAllLoans();
                return View(vm);
            }

            var result = await _repaymentService.RecordRepayment(vm);
            if (result == null)
            {
                ModelState.AddModelError("", "Repayment failed. Check loan ID or amount exceeds balance.");
                ViewBag.Loans = await _loanService.GetAllLoans();
                return View(vm);
            }

            TempData["Success"] = "Repayment recorded successfully.";
            return RedirectToAction(nameof(Index), new { loanId = vm.LoanId });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  CUSTOMER REPAY — Customer only
        // ════════════════════════════════════════════════════════════════════════

        // GET: Repayment/CustomerRepay?loanId=X
        public async Task<IActionResult> CustomerRepay(int loanId)
        {
            if (Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Verify the customer owns this loan
            var loan = await _loanService.GetLoanDetails(loanId);
            if (loan == null || loan.CustomerId != SessionCustomerId)
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Only APPROVED loans can be repaid
            if (loan.LoanStatus != MVC_BANK_FINAL_C.Data.LoanStatus.APPROVED)
            {
                TempData["Error"] = "Only approved loans can be repaid.";
                return RedirectToAction(nameof(Index), new { loanId });
            }

            // Calculate current balance remaining (principal + interest)
            var repayments    = await _repaymentService.GetRepaymentHistory(loanId);
            var lastRepayment = repayments.OrderByDescending(r => r.RepaymentDate).FirstOrDefault();
            decimal totalRepayable   = loan.LoanAmount + (loan.LoanAmount * loan.InterestRate * loan.Tenure / 100m);
            decimal balanceRemaining = lastRepayment?.BalanceRemaining ?? totalRepayable;

            // Guard: fully paid
            if (balanceRemaining <= 0)
            {
                TempData["Error"] = "This loan has already been fully repaid.";
                return RedirectToAction(nameof(Index), new { loanId });
            }

            // Build account dropdown from customer's accounts
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == SessionCustomerId)
                .ToListAsync();

            var vm = new RepaymentViewModel
            {
                LoanId           = loanId,
                BalanceRemaining = balanceRemaining,
                CustomerName     = loan.Customer?.Name ?? "",
                AccountList      = accounts.Select(a => new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text  = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
                }).ToList()
            };

            ViewBag.Loan = loan;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRepay(RepaymentViewModel vm)
        {
            if (Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Verify the customer owns this loan
            var loan = await _loanService.GetLoanDetails(vm.LoanId);
            if (loan == null || loan.CustomerId != SessionCustomerId)
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Recalculate balance remaining (do not trust form post; use principal + interest)
            var repayments    = await _repaymentService.GetRepaymentHistory(vm.LoanId);
            var lastRepayment = repayments.OrderByDescending(r => r.RepaymentDate).FirstOrDefault();
            decimal totalRepayable   = loan.LoanAmount + (loan.LoanAmount * loan.InterestRate * loan.Tenure / 100m);
            decimal balanceRemaining = lastRepayment?.BalanceRemaining ?? totalRepayable;
            vm.BalanceRemaining = balanceRemaining;
            vm.CustomerName     = HttpContext.Session.GetString("Username") ?? "";

            // Validation 1: amount must not exceed loan balance remaining
            if (vm.AmountPaid > balanceRemaining)
            {
                ModelState.AddModelError("AmountPaid",
                    $"Amount cannot exceed the remaining loan balance of {balanceRemaining:C}.");
            }

            // Validation 2: account must exist, belong to this customer, and have enough balance
            if (vm.AccountId.HasValue)
            {
                var account = await _context.Accounts.FindAsync(vm.AccountId.Value);
                if (account == null || account.CustomerId != SessionCustomerId)
                {
                    ModelState.AddModelError("AccountId", "Invalid account selected.");
                }
                else if (account.Balance < vm.AmountPaid)
                {
                    ModelState.AddModelError("AccountId",
                        $"Insufficient balance. Your account balance is {account.Balance:C}.");
                }
            }
            else
            {
                ModelState.AddModelError("AccountId", "Please select an account.");
            }

            if (!ModelState.IsValid)
            {
                var accounts = await _context.Accounts
                    .Where(a => a.CustomerId == SessionCustomerId)
                    .ToListAsync();
                vm.AccountList = accounts.Select(a => new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text  = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
                }).ToList();
                ViewBag.Loan = loan;
                return View(vm);
            }

            var result = await _repaymentService.RecordCustomerRepayment(vm);
            if (result == null)
            {
                ModelState.AddModelError("", "Repayment failed. Please check your inputs and try again.");
                var accounts = await _context.Accounts
                    .Where(a => a.CustomerId == SessionCustomerId)
                    .ToListAsync();
                vm.AccountList = accounts.Select(a => new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text  = $"#{a.AccountId} - {a.AccountType} (Balance: {a.Balance:C})"
                }).ToList();
                ViewBag.Loan = loan;
                return View(vm);
            }

            TempData["Success"] = $"Repayment of {vm.AmountPaid:C} recorded successfully.";
            return RedirectToAction(nameof(Index), new { loanId = vm.LoanId });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  INTEREST — Admin, LoanOfficer, Auditor, Customer (own only)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Interest(int loanId)
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Auditor" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var loan = await _loanService.GetLoanDetails(loanId);
            if (loan == null) return NotFound();

            if (Role == "Customer" && loan.CustomerId != SessionCustomerId)
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var interest = await _repaymentService.CalculateInterest(loanId);
            ViewBag.Interest = interest;
            ViewBag.Loan     = loan;
            return View();
        }
    }
}

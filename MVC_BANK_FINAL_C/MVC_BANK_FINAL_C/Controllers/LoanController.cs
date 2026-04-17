using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Helpers;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class LoanController : Controller
    {
        private readonly ILoanService     _loanService;
        private readonly ICustomerService _customerService;
        private readonly BankingDbContext _context;

        public LoanController(ILoanService loanService, ICustomerService customerService, BankingDbContext context)
        {
            _loanService     = loanService;
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

        // ── Helper: build account SelectList for a given customer ─────────────
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
        //  INDEX — Admin, LoanOfficer, Auditor, Customer (own only)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Auditor" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var all = await _loanService.GetAllLoans();

            if (Role == "Customer")
            {
                int cid = SessionCustomerId ?? 0;
                all = all.Where(l => l.CustomerId == cid);
            }

            // Build a dictionary of loanId -> latest BalanceRemaining for APPROVED loans.
            // Used in the view to detect fully-repaid loans (BalanceRemaining == 0).
            var approvedLoanIds = all
                .Where(l => l.LoanStatus == LoanStatus.APPROVED)
                .Select(l => l.LoanId)
                .ToList();

            var loanBalances = new Dictionary<int, decimal>();
            foreach (var loanId in approvedLoanIds)
            {
                var lastRepayment = await _context.Repayments
                    .Where(r => r.LoanId == loanId)
                    .OrderByDescending(r => r.RepaymentDate)
                    .FirstOrDefaultAsync();

                if (lastRepayment != null)
                    loanBalances[loanId] = lastRepayment.BalanceRemaining;
                // No entry = no repayments yet = not fully paid
            }

            ViewBag.LoanBalances = loanBalances;
            return View(all);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  APPLY GET — Admin, LoanOfficer, Customer
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Apply()
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Customer: ensure they have at least one bank account before applying
            if (Role == "Customer")
            {
                int cid = SessionCustomerId ?? 0;
                bool hasAccount = await _context.Accounts
                    .AnyAsync(a => a.CustomerId == cid);

                if (!hasAccount)
                {
                    TempData["Error"] = "You must have a bank account before applying for a loan. Please contact Admin or Teller to open an account.";
                    return RedirectToAction(nameof(Index));
                }

                // Load customer's accounts for the credit-account dropdown
                ViewBag.AccountList = await BuildAccountList(cid);
            }

            if (Role == "Admin" || Role == "LoanOfficer")
                ViewBag.Customers = await _customerService.GetAllCustomers();

            ViewBag.LoanTypes = LoanInterestHelper.LoanTypes;
            ViewBag.Tenures   = LoanInterestHelper.TenureOptions;
            return View(new LoanViewModel());
        }

        // ════════════════════════════════════════════════════════════════════════
        //  APPLY POST — Admin, LoanOfficer, Customer
        // ════════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LoanViewModel vm)
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            // Customer: auto-assign CustomerId from session and validate credit account
            if (Role == "Customer")
            {
                int cid = SessionCustomerId ?? 0;
                if (cid == 0)
                {
                    TempData["Error"] = "Customer profile not linked to your account.";
                    return RedirectToAction("Index", "Home");
                }
                vm.CustomerId = cid;
                ModelState.Remove("CustomerId");

                // Validate selected credit account belongs to this customer
                if (vm.CreditAccountId.HasValue)
                {
                    bool accountBelongsToCustomer = await _context.Accounts
                        .AnyAsync(a => a.AccountId == vm.CreditAccountId.Value
                                    && a.CustomerId == cid);
                    if (!accountBelongsToCustomer)
                        ModelState.AddModelError("CreditAccountId", "Invalid account selected.");
                }
                else
                {
                    ModelState.AddModelError("CreditAccountId",
                        "Please select an account to receive the loan amount.");
                }
            }

            // Admin / LoanOfficer: validate credit account belongs to selected customer
            if (Role == "Admin" || Role == "LoanOfficer")
            {
                if (vm.CreditAccountId.HasValue)
                {
                    bool accountBelongsToCustomer = await _context.Accounts
                        .AnyAsync(a => a.AccountId == vm.CreditAccountId.Value
                                    && a.CustomerId == vm.CustomerId);
                    if (!accountBelongsToCustomer)
                        ModelState.AddModelError("CreditAccountId", "Invalid account selected.");
                }
                else
                {
                    ModelState.AddModelError("CreditAccountId",
                        "Please select an account to receive the loan amount.");
                }
            }

            // Auto-calculate interest and EMI
            vm.InterestRate = LoanInterestHelper.GetInterestRate(vm.LoanType);
            vm.MonthlyEMI   = LoanInterestHelper.CalculateEMI(vm.LoanAmount, vm.InterestRate, vm.Tenure);
            ModelState.Remove("InterestRate");
            ModelState.Remove("MonthlyEMI");

            if (!ModelState.IsValid)
            {
                if (Role == "Admin" || Role == "LoanOfficer")
                    ViewBag.Customers = await _customerService.GetAllCustomers();

                if (Role == "Customer")
                    ViewBag.AccountList = await BuildAccountList(SessionCustomerId ?? 0);

                ViewBag.LoanTypes = LoanInterestHelper.LoanTypes;
                ViewBag.Tenures   = LoanInterestHelper.TenureOptions;
                return View(vm);
            }

            await _loanService.ApplyLoan(vm);
            TempData["Success"] = $"Loan application submitted. Monthly EMI: {vm.MonthlyEMI:C}";
            return RedirectToAction(nameof(Index));
        }

        // ════════════════════════════════════════════════════════════════════════
        //  APPROVE — Admin, LoanOfficer
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Approve(int id)
        {
            if (Role != "Admin" && Role != "LoanOfficer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var loan = await _loanService.GetLoanDetails(id);
            if (loan == null) return NotFound();
            return View(loan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string decision)
        {
            if (Role != "Admin" && Role != "LoanOfficer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var result = await _loanService.ApproveLoan(id, decision);
            if (result == null) return NotFound();

            TempData["Success"] = $"Loan {decision}d successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ════════════════════════════════════════════════════════════════════════
        //  DETAILS — Admin, LoanOfficer, Auditor, Customer (own only)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            if (Role != "Admin" && Role != "LoanOfficer" && Role != "Auditor" && Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var loan = await _loanService.GetLoanDetails(id);
            if (loan == null) return NotFound();

            // Customer may only view own loans
            if (Role == "Customer" && loan.CustomerId != SessionCustomerId)
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            return View(loan);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  AJAX — interest rate auto-fill
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetInterestRate(string loanType)
        {
            var rate = LoanInterestHelper.GetInterestRate(loanType);
            return Json(rate);
        }
    }
}

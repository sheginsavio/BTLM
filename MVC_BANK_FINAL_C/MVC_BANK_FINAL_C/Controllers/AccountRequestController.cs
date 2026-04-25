using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class AccountRequestController : Controller
    {
        private readonly BankingDbContext _context;

        public AccountRequestController(BankingDbContext context)
        {
            _context = context;
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
        private string Username =>
            HttpContext.Session.GetString("Username") ?? "System";

        // ── Customer: Submit new account request ──────────────────

        // GET: AccountRequest/Request
        public IActionResult Request()
        {
            if (Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            return View(new AccountRequestViewModel
            {
                CustomerId = SessionCustomerId ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(AccountRequestViewModel vm)
        {
            if (Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            vm.CustomerId = SessionCustomerId ?? 0;

            if (!ModelState.IsValid) return View(vm);

            // Check if customer already has a PENDING request for same type
            bool pendingExists = await _context.AccountRequests
                .AnyAsync(r => r.CustomerId == vm.CustomerId
                            && r.AccountType == vm.AccountType
                            && r.Status == "PENDING");
            if (pendingExists)
            {
                ModelState.AddModelError("",
                    "You already have a pending request for this account type.");
                return View(vm);
            }

            var request = new AccountRequest
            {
                CustomerId  = vm.CustomerId,
                AccountType = vm.AccountType,
                Status      = "PENDING",
                RequestedOn = DateTime.Now
            };
            _context.AccountRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Account request submitted successfully! " +
                "Please wait for Admin/Teller approval.";
            return RedirectToAction(nameof(MyRequests));
        }

        // ── Customer: View their own requests ────────────────────

        public async Task<IActionResult> MyRequests()
        {
            if (Role != "Customer")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            int cid = SessionCustomerId ?? 0;
            var requests = await _context.AccountRequests
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == cid)
                .OrderByDescending(r => r.RequestedOn)
                .ToListAsync();

            return View(requests);
        }

        // ── Admin, Teller: View all requests ─────────────────────

        public async Task<IActionResult> PendingRequests()
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var requests = await _context.AccountRequests
                .Include(r => r.Customer)
                .OrderByDescending(r => r.RequestedOn)
                .ToListAsync();

            return View(requests);
        }

        // ── Admin, Teller: Approve request ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int requestId)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var request = await _context.AccountRequests
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null) return NotFound();

            if (request.Status != "PENDING")
            {
                TempData["Error"] = "This request has already been processed.";
                return RedirectToAction(nameof(PendingRequests));
            }

            // Create the actual account
            var account = new Account
            {
                CustomerId  = request.CustomerId,
                AccountType = request.AccountType,
                Balance     = 0
            };
            _context.Accounts.Add(account);

            // Update request status
            request.Status     = "APPROVED";
            request.ReviewedOn = DateTime.Now;
            request.ReviewedBy = Username;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Account request approved. " +
                $"{request.AccountType} account created for {request.Customer?.Name}.";
            return RedirectToAction(nameof(PendingRequests));
        }

        // ── Admin, Teller: Reject request (GET) ──────────────────

        public async Task<IActionResult> Reject(int requestId)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var request = await _context.AccountRequests
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null) return NotFound();

            var vm = new AccountRequestViewModel
            {
                RequestId    = request.RequestId,
                CustomerId   = request.CustomerId,
                CustomerName = request.Customer?.Name ?? "",
                AccountType  = request.AccountType,
                Status       = request.Status
            };
            return View(vm);
        }

        // ── Admin, Teller: Reject request (POST) ─────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Reject")]
        public async Task<IActionResult> RejectPost(AccountRequestViewModel vm)
        {
            if (Role != "Admin" && Role != "Teller")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var request = await _context.AccountRequests
                .FirstOrDefaultAsync(r => r.RequestId == vm.RequestId);

            if (request == null) return NotFound();

            if (string.IsNullOrWhiteSpace(vm.RejectionReason))
            {
                ModelState.AddModelError("RejectionReason",
                    "Rejection reason is required.");
                return View(vm);
            }

            request.Status          = "REJECTED";
            request.ReviewedOn      = DateTime.Now;
            request.ReviewedBy      = Username;
            request.RejectionReason = vm.RejectionReason;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Account request rejected.";
            return RedirectToAction(nameof(PendingRequests));
        }
    }
}

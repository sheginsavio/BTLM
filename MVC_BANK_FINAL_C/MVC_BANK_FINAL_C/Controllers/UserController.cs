using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Helpers;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class UserController : Controller
    {
        private readonly BankingDbContext _context;

        public UserController(BankingDbContext context)
        {
            _context = context;
        }

        private string Role =>
            HttpContext.Session.GetString("UserRole") ?? "";
        private string Username =>
            HttpContext.Session.GetString("Username") ?? "Admin";

        // ── Admin only: List all users ────────────────────────────

        public async Task<IActionResult> Index()
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var users = await _context.Users
                .Include(u => u.Customer)
                .OrderBy(u => u.Role)
                .ToListAsync();

            return View(users);
        }

        // ── Admin only: Create staff user ─────────────────────────

        public IActionResult Create()
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            bool usernameTaken = await _context.Users
                .AnyAsync(u => u.Username == vm.Username);
            if (usernameTaken)
            {
                ModelState.AddModelError("Username",
                    "This username is already taken.");
            }

            if (!ModelState.IsValid) return View(vm);

            var user = new User
            {
                Username     = vm.Username,
                Password     = PasswordHelper.HashPassword(vm.Password),
                Role         = vm.Role,
                IsFirstLogin = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"User '{vm.Username}' created with role {vm.Role}.";
            return RedirectToAction(nameof(Index));
        }

        // ── Admin only: Reset password for any user ───────────────

        public async Task<IActionResult> ResetPassword(int id)
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(new AdminResetPasswordViewModel
            {
                UserId   = id,
                Username = user.Username,
                Role     = user.Role
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel vm)
        {
            if (Role != "Admin")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.FindAsync(vm.UserId);
            if (user == null) return NotFound();

            user.Password         = PasswordHelper.HashPassword(vm.NewPassword);
            user.SecurityQuestion = vm.SecurityQuestion;
            user.SecurityAnswer   = string.IsNullOrWhiteSpace(vm.SecurityAnswer)
                ? user.SecurityAnswer
                : PasswordHelper.HashPassword(vm.SecurityAnswer.ToLower().Trim());
            user.IsFirstLogin     = true;
            // Force user to change password on next login

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Password reset for '{user.Username}'. " +
                $"They will be prompted to change it on next login.";
            return RedirectToAction(nameof(Index));
        }
    }
}

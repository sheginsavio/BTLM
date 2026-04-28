using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Helpers;
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

        //  Login 

        [AllowAnonymous]
        public IActionResult Login()
        {
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
                .FirstOrDefaultAsync(u => u.Username == vm.Username);

            if (user == null || !PasswordHelper.VerifyPassword(vm.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(vm);
            }

            HttpContext.Session.SetString("UserId",   user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.CustomerId.HasValue)
                HttpContext.Session.SetString("CustomerId", user.CustomerId.Value.ToString());

            // Check if first login — force password + security question setup
            if (user.IsFirstLogin)
            {
                TempData["FirstLoginUserId"] = user.UserId.ToString();
                return RedirectToAction(nameof(FirstLoginSetup));
            }

            return RedirectToAction("Index", "Home");
        }

        //  Register 

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

            bool usernameTaken = await _context.Users.AnyAsync(u => u.Username == vm.Username);
            if (usernameTaken)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(vm);
            }

            bool emailTaken = await _context.Customers
                .AnyAsync(c => c.Email == vm.Email);
            if (emailTaken)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(vm);
            }

            bool mobileTaken = await _context.Customers
                .AnyAsync(c => c.ContactInfo == vm.ContactInfo);
            if (mobileTaken)
            {
                ModelState.AddModelError("ContactInfo",
                    "This phone number is already registered.");
                return View(vm);
            }

            var customer = new Customer
            {
                Name        = vm.Name,
                Email       = vm.Email,
                ContactInfo = vm.ContactInfo
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username         = vm.Username,
                Password         = PasswordHelper.HashPassword(vm.Password),
                Role             = "Customer",
                CustomerId       = customer.CustomerId,
                SecurityQuestion = vm.SecurityQuestion,
                SecurityAnswer   = PasswordHelper.HashPassword(vm.SecurityAnswer.ToLower().Trim()),
                IsFirstLogin     = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // Logout 

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

        //  Forgot Password — Step 1: Enter Username 

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordStep1ViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == vm.Username
                                       && u.Role == "Customer");

            if (user == null)
            {
                ModelState.AddModelError("Username",
                    "No customer account found with this username.");
                return View(vm);
            }

            if (string.IsNullOrEmpty(user.SecurityQuestion))
            {
                ModelState.AddModelError("Username",
                    "This account does not have a security question set. Please contact Admin.");
                return View(vm);
            }

            TempData["FPUsername"] = vm.Username;
            return RedirectToAction(nameof(ForgotPasswordStep2));
        }

        // Forgot Password — Step 2: Security Question 

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordStep2()
        {
            var username = TempData["FPUsername"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction(nameof(ForgotPassword));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.Role == "Customer");
            if (user == null)
                return RedirectToAction(nameof(ForgotPassword));

            TempData.Keep("FPUsername");

            var vm = new ForgotPasswordStep2ViewModel
            {
                Username         = username,
                SecurityQuestion = user.SecurityQuestion ?? ""
            };
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordStep2(ForgotPasswordStep2ViewModel vm)
        {
            var username = TempData["FPUsername"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction(nameof(ForgotPassword));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.Role == "Customer");
            if (user == null)
                return RedirectToAction(nameof(ForgotPassword));

            if (!ModelState.IsValid)
            {
                vm.Username         = username;
                vm.SecurityQuestion = user.SecurityQuestion ?? "";
                TempData.Keep("FPUsername");
                return View(vm);
            }

            //string hashedInput = PasswordHelper.HashPassword(
            //    vm.SecurityAnswer.ToLower().Trim());

            //if (hashedInput != user.SecurityAnswer)
            if(!PasswordHelper.VerifySecurityAnswer(vm.SecurityAnswer.ToLower().Trim(), user.SecurityAnswer))
            {
                ModelState.AddModelError("SecurityAnswer",
                    "Incorrect answer. Please try again.");
                vm.Username         = username;
                vm.SecurityQuestion = user.SecurityQuestion ?? "";
                TempData.Keep("FPUsername");
                return View(vm);
            }

            TempData["FPUsername"] = username;
            TempData["FPVerified"] = "true";
            return RedirectToAction(nameof(ForgotPasswordStep3));
        }

        // Forgot Password — Step 3: Reset Password 

        [AllowAnonymous]
        public IActionResult ForgotPasswordStep3()
        {
            var username = TempData["FPUsername"] as string;
            var verified = TempData["FPVerified"] as string;

            if (string.IsNullOrEmpty(username) || verified != "true")
                return RedirectToAction(nameof(ForgotPassword));

            TempData.Keep("FPUsername");
            TempData.Keep("FPVerified");

            return View(new ForgotPasswordStep3ViewModel { Username = username });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordStep3(ForgotPasswordStep3ViewModel vm)
        {
            var username = TempData["FPUsername"] as string;
            var verified = TempData["FPVerified"] as string;

            if (string.IsNullOrEmpty(username) || verified != "true")
                return RedirectToAction(nameof(ForgotPassword));

            if (!ModelState.IsValid)
            {
                TempData.Keep("FPUsername");
                TempData.Keep("FPVerified");
                return View(vm);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.Role == "Customer");
            if (user == null)
                return RedirectToAction(nameof(ForgotPassword));

            user.Password = PasswordHelper.HashPassword(vm.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password reset successfully! Please log in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        // First Login Setup 

        public IActionResult FirstLoginSetup()
        {
            var userId = HttpContext.Session.GetString("UserId")
                      ?? TempData["FirstLoginUserId"] as string;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login));

            TempData.Keep("FirstLoginUserId");
            return View(new FirstLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FirstLoginSetup(FirstLoginViewModel vm)
        {
            var userId = HttpContext.Session.GetString("UserId")
                      ?? TempData["FirstLoginUserId"] as string;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                TempData.Keep("FirstLoginUserId");
                return View(vm);
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                return RedirectToAction(nameof(Login));

            user.Password         = PasswordHelper.HashPassword(vm.NewPassword);
            user.SecurityQuestion = vm.SecurityQuestion;
            user.SecurityAnswer   = PasswordHelper.HashPassword(
                                        vm.SecurityAnswer.ToLower().Trim());
            user.IsFirstLogin     = false;

            await _context.SaveChangesAsync();

            // Set session if not already set (coming from first login redirect)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                HttpContext.Session.SetString("UserId",   user.UserId.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);
                if (user.CustomerId.HasValue)
                    HttpContext.Session.SetString("CustomerId",
                        user.CustomerId.Value.ToString());
            }

            TempData["Success"] = "Password and security question set successfully! Welcome!";
            return RedirectToAction("Index", "Home");
        }
    }
}

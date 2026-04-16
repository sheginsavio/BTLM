using Microsoft.AspNetCore.Mvc;
using MVC_BANK_FINAL_C.Models;
using MVC_BANK_FINAL_C.Services.Interfaces;
using System.Diagnostics;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportService _reportService;

        public HomeController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var role       = HttpContext.Session.GetString("UserRole") ?? "Admin";
            var custIdStr  = HttpContext.Session.GetString("CustomerId");
            int? customerId = int.TryParse(custIdStr, out int cid) ? cid : null;

            var dashboard = await _reportService.GetRoleDashboard(role, customerId);
            ViewBag.Role = role;
            return View(dashboard);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

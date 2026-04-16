using Microsoft.AspNetCore.Mvc;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private string Role => HttpContext.Session.GetString("UserRole") ?? "";

        // Admin, Auditor only
        public async Task<IActionResult> Transactions(DateTime? from, DateTime? to)
        {
            if (Role != "Admin" && Role != "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.From = from;
            ViewBag.To   = to;
            var transactions = await _reportService.GenerateTransactionReport(from, to);
            return View(transactions);
        }

        // Admin, Auditor only
        public async Task<IActionResult> AuditLogs()
        {
            if (Role != "Admin" && Role != "Auditor")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Home");
            }
            var logs = await _reportService.GetAuditLogs();
            return View(logs);
        }
    }
}

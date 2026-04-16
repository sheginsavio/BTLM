using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<Transaction>> GenerateTransactionReport(DateTime? from, DateTime? to);
        Task<IEnumerable<AuditLog>> GetAuditLogs();
        Task<DashboardViewModel> GetDashboardSummary();
        Task<DashboardViewModel> GetRoleDashboard(string role, int? customerId = null);
    }
}

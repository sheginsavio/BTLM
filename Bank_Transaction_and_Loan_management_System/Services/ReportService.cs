using Bank_Transaction_and_Loan_management_System.Interfaces;
using Bank_Transaction_and_Loan_management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank_Transaction_and_Loan_management_System.Services
{
    public class ReportService : IReportService
    {
        private readonly BankingDbContext _context;

        public ReportService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<BankTransaction>> GetAllTransactionsAsync(string userRole)
        {
            try
            {
                // Only Admins can view all transactions
                if (userRole != "Admin")
                    throw new UnauthorizedAccessException("Only admins can access reports.");

                return await _context.BankTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve transactions.", ex);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string userRole)
        {
            try
            {
                // Only Admins can view audit logs
                if (userRole != "Admin")
                    throw new UnauthorizedAccessException("Only admins can access reports.");

                return await _context.AuditLogs
                    .OrderByDescending(a => a.LogDate)
                    .ToListAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve audit logs.", ex);
            }
        }
    }
}

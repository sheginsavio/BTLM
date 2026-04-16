using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly BankingDbContext _context;

        public ReportService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GenerateTransactionReport(DateTime? from, DateTime? to)
        {
            var query = _context.Transactions
                .Include(t => t.Account)
                    .ThenInclude(a => a!.Customer)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(t => t.TransactionDate >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.TransactionDate <= to.Value.AddDays(1));

            return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogs()
        {
            return await _context.AuditLogs
                .Include(al => al.Transaction)
                    .ThenInclude(t => t!.Account)
                        .ThenInclude(a => a!.Customer)
                .OrderByDescending(al => al.LogDate)
                .ToListAsync();
        }

        public async Task<DashboardViewModel> GetDashboardSummary()
        {
            return new DashboardViewModel
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalTransactions = await _context.Transactions.CountAsync(),
                ActiveLoans = await _context.Loans.CountAsync(l => l.LoanStatus == LoanStatus.APPROVED),
                TotalBalance = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0
            };
        }

        public async Task<DashboardViewModel> GetRoleDashboard(string role, int? customerId = null)
        {
            var vm = new DashboardViewModel();

            switch (role)
            {
                case "Admin":
                    vm.TotalCustomers = await _context.Customers.CountAsync();
                    vm.TotalTransactions = await _context.Transactions.CountAsync();
                    vm.ActiveLoans = await _context.Loans.CountAsync(l => l.LoanStatus == LoanStatus.APPROVED);
                    vm.TotalBalance = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0;
                    vm.TotalUsers = await _context.Users.CountAsync();
                    break;

                case "Teller":
                    vm.TotalTransactions = await _context.Transactions.CountAsync();
                    vm.RecentDeposits = await _context.Transactions
                        .Include(t => t.Account).ThenInclude(a => a!.Customer)
                        .Where(t => t.TransactionType == TransactionType.DEPOSIT)
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .ToListAsync();
                    vm.RecentWithdrawals = await _context.Transactions
                        .Include(t => t.Account).ThenInclude(a => a!.Customer)
                        .Where(t => t.TransactionType == TransactionType.WITHDRAWAL)
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .ToListAsync();
                    break;

                case "LoanOfficer":
                    vm.PendingLoans = await _context.Loans.CountAsync(l => l.LoanStatus == LoanStatus.APPLIED);
                    vm.ApprovedLoans = await _context.Loans.CountAsync(l => l.LoanStatus == LoanStatus.APPROVED);
                    vm.RejectedLoans = await _context.Loans.CountAsync(l => l.LoanStatus == LoanStatus.REJECTED);
                    vm.RecentLoans = await _context.Loans
                        .Include(l => l.Customer)
                        .OrderByDescending(l => l.LoanId)
                        .Take(5)
                        .ToListAsync();
                    break;

                case "Auditor":
                    vm.TotalTransactions = await _context.Transactions.CountAsync();
                    vm.RecentAuditLogs = await _context.AuditLogs
                        .Include(al => al.Transaction)
                            .ThenInclude(t => t!.Account)
                                .ThenInclude(a => a!.Customer)
                        .OrderByDescending(al => al.LogDate)
                        .Take(10)
                        .ToListAsync();
                    break;

                case "Customer":
                    if (customerId.HasValue)
                    {
                        int cid = customerId.Value;

                        var accountIds = await _context.Accounts
                            .Where(a => a.CustomerId == cid)
                            .Select(a => a.AccountId)
                            .ToListAsync();

                        vm.OwnBalance = await _context.Accounts
                            .Where(a => a.CustomerId == cid)
                            .SumAsync(a => (decimal?)a.Balance) ?? 0;

                        vm.OwnRecentTransactions = await _context.Transactions
                            .Include(t => t.Account)
                            .Where(t => accountIds.Contains(t.AccountId))
                            .OrderByDescending(t => t.TransactionDate)
                            .Take(5)
                            .ToListAsync();

                        vm.OwnActiveLoan = await _context.Loans
                            .Where(l => l.CustomerId == cid && l.LoanStatus == LoanStatus.APPROVED)
                            .OrderByDescending(l => l.LoanId)
                            .FirstOrDefaultAsync();
                    }
                    break;

                default:
                    vm = await GetDashboardSummary();
                    break;
            }

            return vm;
        }
    }
}

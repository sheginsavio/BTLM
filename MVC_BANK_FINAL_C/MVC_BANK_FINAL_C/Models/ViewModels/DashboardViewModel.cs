using MVC_BANK_FINAL_C.Models.Entities;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class DashboardViewModel
    {
        // ── Shared / Admin ─────────────────────────────────────
        public int TotalCustomers { get; set; }
        public int TotalTransactions { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalUsers { get; set; }

        // ── Teller ─────────────────────────────────────────────
        public IEnumerable<Transaction> RecentDeposits { get; set; } = new List<Transaction>();
        public IEnumerable<Transaction> RecentWithdrawals { get; set; } = new List<Transaction>();

        // ── LoanOfficer ────────────────────────────────────────
        public int PendingLoans { get; set; }
        public int ApprovedLoans { get; set; }
        public int RejectedLoans { get; set; }
        public IEnumerable<Loan> RecentLoans { get; set; } = new List<Loan>();

        // ── Auditor ────────────────────────────────────────────
        public IEnumerable<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();

        // ── Customer ───────────────────────────────────────────
        public decimal OwnBalance { get; set; }
        public IEnumerable<Transaction> OwnRecentTransactions { get; set; } = new List<Transaction>();
        public Loan? OwnActiveLoan { get; set; }
    }
}

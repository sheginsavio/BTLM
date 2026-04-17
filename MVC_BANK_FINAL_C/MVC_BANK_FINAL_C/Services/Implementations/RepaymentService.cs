using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Services.Implementations
{
    public class RepaymentService : IRepaymentService
    {
        private readonly BankingDbContext _context;

        public RepaymentService(BankingDbContext context)
        {
            _context = context;
        }

        // ── Staff-recorded repayment (Admin / LoanOfficer) ───────────────────
        public async Task<Repayment?> RecordRepayment(RepaymentViewModel vm)
        {
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Repayments)
                    .FirstOrDefaultAsync(l => l.LoanId == vm.LoanId);

                if (loan == null) return null;

                // Determine current balance remaining (principal + interest)
                var lastRepayment = loan.Repayments.OrderByDescending(r => r.RepaymentDate).FirstOrDefault();
                decimal totalRepayable = GetTotalRepayable(loan);
                decimal currentBalance = lastRepayment?.BalanceRemaining ?? totalRepayable;

                if (vm.AmountPaid > currentBalance) return null;

                decimal newBalance = currentBalance - vm.AmountPaid;

                var repayment = new Repayment
                {
                    LoanId           = vm.LoanId,
                    RepaymentDate    = DateTime.Now,
                    AmountPaid       = vm.AmountPaid,
                    BalanceRemaining = newBalance
                };
                _context.Repayments.Add(repayment);
                await _context.SaveChangesAsync();
                return repayment;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ── Customer self-service repayment ───────────────────────────────────
        public async Task<Repayment?> RecordCustomerRepayment(RepaymentViewModel vm)
        {
            try
            {
                // Load loan with its repayment history
                var loan = await _context.Loans
                    .Include(l => l.Repayments)
                    .FirstOrDefaultAsync(l => l.LoanId == vm.LoanId);
                if (loan == null) return null;

                // Only APPROVED loans can be repaid
                if (loan.LoanStatus != LoanStatus.APPROVED) return null;

                // Calculate current balance remaining (principal + interest)
                var lastRepayment = loan.Repayments
                    .OrderByDescending(r => r.RepaymentDate)
                    .FirstOrDefault();
                decimal totalRepayable = GetTotalRepayable(loan);
                decimal currentBalance = lastRepayment?.BalanceRemaining ?? totalRepayable;

                // Validation 1: amount must not exceed loan balance remaining
                if (vm.AmountPaid > currentBalance) return null;

                // Load the source account
                var account = await _context.Accounts.FindAsync(vm.AccountId);
                if (account == null) return null;

                // Validation 2: account must have sufficient balance
                if (account.Balance < vm.AmountPaid) return null;

                // Deduct from account balance
                account.Balance -= vm.AmountPaid;

                // Record the repayment
                decimal newBalance = currentBalance - vm.AmountPaid;
                var repayment = new Repayment
                {
                    LoanId           = vm.LoanId,
                    RepaymentDate    = DateTime.Now,
                    AmountPaid       = vm.AmountPaid,
                    BalanceRemaining = newBalance
                };
                _context.Repayments.Add(repayment);

                // Note: AuditLog is skipped here because AuditLog.TransactionId is a
                // required non-nullable FK to the Transactions table, and customer
                // repayments do not generate a Transaction row.

                await _context.SaveChangesAsync();
                return repayment;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the total amount a customer must repay:
        /// Principal + Simple Interest (P × R × T / 100).
        /// </summary>
        private static decimal GetTotalRepayable(Loan loan)
        {
            return loan.LoanAmount + (loan.LoanAmount * loan.InterestRate * loan.Tenure / 100m);
        }

        // ── Interest calculation ──────────────────────────────────────────────
        public async Task<decimal> CalculateInterest(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return 0;

            // Simple Interest: P × R × T / 100  — uses the loan's actual Tenure
            int tenure = loan.Tenure > 0 ? loan.Tenure : 1;
            decimal interest = loan.LoanAmount * loan.InterestRate * tenure / 100m;
            return Math.Round(interest, 2);
        }

        // ── Repayment history ─────────────────────────────────────────────────
        public async Task<IEnumerable<Repayment>> GetRepaymentHistory(int loanId)
        {
            return await _context.Repayments
                .Where(r => r.LoanId == loanId)
                .OrderByDescending(r => r.RepaymentDate)
                .ToListAsync();
        }
    }
}

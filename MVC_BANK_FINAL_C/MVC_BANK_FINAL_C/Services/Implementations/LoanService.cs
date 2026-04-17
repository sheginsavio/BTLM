using Microsoft.EntityFrameworkCore;
using MVC_BANK_FINAL_C.Data;
using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;
using MVC_BANK_FINAL_C.Services.Interfaces;

namespace MVC_BANK_FINAL_C.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly BankingDbContext _context;

        public LoanService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Loan> ApplyLoan(LoanViewModel vm)
        {
            var loan = new Loan
            {
                CustomerId      = vm.CustomerId,
                LoanAmount      = vm.LoanAmount,
                InterestRate    = vm.InterestRate,
                LoanStatus      = LoanStatus.APPLIED,
                LoanType        = vm.LoanType ?? "Personal",
                Tenure          = vm.Tenure > 0 ? vm.Tenure : 1,
                MonthlyEMI      = vm.MonthlyEMI,
                CreditAccountId = vm.CreditAccountId
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<Loan?> ApproveLoan(int loanId, string decision)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null) return null;

                loan.LoanStatus = decision.ToUpper() == "APPROVE"
                    ? LoanStatus.APPROVED
                    : LoanStatus.REJECTED;

                // On approval, credit the loan amount to the customer's chosen account
                if (loan.LoanStatus == LoanStatus.APPROVED && loan.CreditAccountId.HasValue)
                {
                    var account = await _context.Accounts.FindAsync(loan.CreditAccountId.Value);
                    if (account != null)
                    {
                        account.Balance += loan.LoanAmount;

                        // Note: AuditLog is skipped here because AuditLog.TransactionId is a
                        // required non-nullable FK to the Transactions table, and this operation
                        // does not create a Transaction row.
                    }
                }

                await _context.SaveChangesAsync();
                return loan;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Loan?> GetLoanDetails(int loanId)
        {
            return await _context.Loans
                .Include(l => l.Customer)
                .Include(l => l.Repayments)
                .Include(l => l.CreditAccount)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);
        }

        public async Task<IEnumerable<Loan>> GetAllLoans()
        {
            return await _context.Loans
                .Include(l => l.Customer)
                .ToListAsync();
        }
    }
}

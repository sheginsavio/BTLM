using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Services.Interfaces
{
    public interface ILoanService
    {
        Task<Loan> ApplyLoan(LoanViewModel vm);
        Task<Loan?> ApproveLoan(int loanId, string decision);
        Task<Loan?> GetLoanDetails(int loanId);
        Task<IEnumerable<Loan>> GetAllLoans();
    }
}

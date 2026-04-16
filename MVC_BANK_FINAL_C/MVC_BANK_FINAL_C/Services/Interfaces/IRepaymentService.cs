using MVC_BANK_FINAL_C.Models.Entities;
using MVC_BANK_FINAL_C.Models.ViewModels;

namespace MVC_BANK_FINAL_C.Services.Interfaces
{
    public interface IRepaymentService
    {
        Task<Repayment?> RecordRepayment(RepaymentViewModel vm);
        Task<Repayment?> RecordCustomerRepayment(RepaymentViewModel vm);
        Task<decimal> CalculateInterest(int loanId);
        Task<IEnumerable<Repayment>> GetRepaymentHistory(int loanId);
    }
}

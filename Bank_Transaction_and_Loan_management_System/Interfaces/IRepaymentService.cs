using Bank_Transaction_and_Loan_management_System.Models;

namespace Bank_Transaction_and_Loan_management_System.Interfaces
{
    public interface IRepaymentService
    {
        Task<bool> RecordRepaymentAsync(int loanId, decimal amount);
        Task<List<Repayment>> GetRepaymentHistoryAsync(int loanId);
    }
}

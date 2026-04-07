using Bank_Transaction_and_Loan_management_System.Models;

namespace Bank_Transaction_and_Loan_management_System.Interfaces
{
    public interface ILoanService
    {
        Task<LoanDto> ApplyLoanAsync(int customerId, decimal loanAmount, decimal interestRate, int userId);
        Task<bool> ApproveLoanAsync(int loanId, string userRole);
        Task<bool> RejectLoanAsync(int loanId, string userRole);
        Task<LoanDto?> GetLoanByIdAsync(int loanId, int userId, string userRole);
        Task<List<LoanDto>> GetUserLoansAsync(int userId);
    }

    public class LoanDto
    {
        public int LoanId { get; set; }
        public int CustomerId { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public string LoanStatus { get; set; } = null!;
    }
}

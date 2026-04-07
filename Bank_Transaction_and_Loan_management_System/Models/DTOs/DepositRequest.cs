using System.ComponentModel.DataAnnotations;

namespace Bank_Transaction_and_Loan_management_System.Models.DTOs
{
    public class DepositRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AccountId must be greater than 0")]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}

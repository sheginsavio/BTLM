using System.ComponentModel.DataAnnotations;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class LoanViewModel
    {
        public int LoanId { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Loan type is required")]
        [Display(Name = "Loan Type")]
        public string LoanType { get; set; } = "Personal";

        [Required(ErrorMessage = "Loan amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Loan amount must be greater than zero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Loan Amount")]
        public decimal LoanAmount { get; set; }

        [Display(Name = "Interest Rate (%)")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Tenure is required")]
        [Range(1, 30, ErrorMessage = "Tenure must be between 1 and 30 years")]
        [Display(Name = "Tenure (Years)")]
        public int Tenure { get; set; } = 1;

        [Display(Name = "Monthly EMI")]
        public decimal MonthlyEMI { get; set; }

        [Display(Name = "Loan Status")]
        public LoanStatus LoanStatus { get; set; } = LoanStatus.APPLIED;
    }
}

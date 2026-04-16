using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "Loan amount must be greater than zero")]
        [Display(Name = "Loan Amount")]
        public decimal LoanAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01 and 100")]
        [Display(Name = "Interest Rate (%)")]
        public decimal InterestRate { get; set; }

        [Required]
        [Display(Name = "Loan Status")]
        public LoanStatus LoanStatus { get; set; } = LoanStatus.APPLIED;

        // --- New fields ---
        [StringLength(50)]
        [Display(Name = "Loan Type")]
        public string LoanType { get; set; } = "Personal";

        [Range(1, 30)]
        [Display(Name = "Tenure (Years)")]
        public int Tenure { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monthly EMI")]
        public decimal MonthlyEMI { get; set; } = 0;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
    }
}

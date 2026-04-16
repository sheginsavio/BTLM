using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class Repayment
    {
        [Key]
        public int RepaymentId { get; set; }

        [Required]
        [Display(Name = "Loan")]
        public int LoanId { get; set; }

        [Required]
        [Display(Name = "Repayment Date")]
        public DateTime RepaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than zero")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance remaining cannot be negative")]
        [Display(Name = "Balance Remaining")]
        public decimal BalanceRemaining { get; set; }

        // Navigation properties
        [ForeignKey("LoanId")]
        public Loan? Loan { get; set; }
    }
}

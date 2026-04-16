using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Account")]
        public int AccountId { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AccountId")]
        public Account? Account { get; set; }

        public AuditLog? AuditLog { get; set; }
    }
}

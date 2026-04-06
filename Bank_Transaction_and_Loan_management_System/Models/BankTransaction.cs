using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank_Transaction_and_Loan_management_System.Models
{
    public class BankTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        public Account? Account { get; set; }
    }
}

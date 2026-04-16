using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVC_BANK_FINAL_C.Data;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
        public decimal Balance { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

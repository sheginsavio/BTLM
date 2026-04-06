using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank_Transaction_and_Loan_management_System.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public Customer Customer { get; set; }
    }
}

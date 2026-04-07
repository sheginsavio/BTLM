using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank_Transaction_and_Loan_management_System.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactInfo { get; set; }

        public User? User { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Loan> Loans { get; set; } = new List<Loan>();
    }
}

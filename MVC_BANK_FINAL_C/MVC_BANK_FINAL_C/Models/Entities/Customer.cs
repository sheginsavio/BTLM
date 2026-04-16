using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Contact info cannot exceed 200 characters")]
        [Display(Name = "Contact Information")]
        public string? ContactInfo { get; set; }

        // Navigation properties
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}

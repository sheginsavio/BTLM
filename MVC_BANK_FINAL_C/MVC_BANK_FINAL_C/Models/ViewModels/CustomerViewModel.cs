using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Contact Information")]
        public string? ContactInfo { get; set; }
    }
}

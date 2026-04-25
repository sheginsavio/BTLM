using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 digits")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain numbers only")]
        [Display(Name = "Phone Number")]
        public string ContactInfo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Security question is required")]
        [StringLength(200, ErrorMessage = "Question cannot exceed 200 characters")]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Security answer is required")]
        [StringLength(100, ErrorMessage = "Answer cannot exceed 100 characters")]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; } = string.Empty;
    }
}

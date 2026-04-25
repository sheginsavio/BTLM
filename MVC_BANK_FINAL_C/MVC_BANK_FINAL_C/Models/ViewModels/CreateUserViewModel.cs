using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Teller";
        // Allowed: Teller, LoanOfficer, Auditor (NOT Admin, NOT Customer)
    }
}

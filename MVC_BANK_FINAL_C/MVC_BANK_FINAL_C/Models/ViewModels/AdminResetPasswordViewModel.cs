using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class AdminResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Security Question")]
        public string? SecurityQuestion { get; set; }

        [StringLength(100)]
        [Display(Name = "Security Answer")]
        public string? SecurityAnswer { get; set; }
    }
}

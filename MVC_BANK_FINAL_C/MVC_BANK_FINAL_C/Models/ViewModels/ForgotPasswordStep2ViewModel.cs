using System.ComponentModel.DataAnnotations;

namespace MVC_BANK_FINAL_C.Models.ViewModels
{
    public class ForgotPasswordStep2ViewModel
    {
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Security answer is required")]
        [Display(Name = "Your Answer")]
        public string SecurityAnswer { get; set; } = string.Empty;
    }
}

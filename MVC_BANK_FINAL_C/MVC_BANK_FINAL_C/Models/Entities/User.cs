using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_BANK_FINAL_C.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;

        // Nullable FK to Customer (only for Customer role)
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [StringLength(200)]
        [Display(Name = "Security Question")]
        public string? SecurityQuestion { get; set; }

        [StringLength(256)]
        [Display(Name = "Security Answer")]
        public string? SecurityAnswer { get; set; }

        public bool IsFirstLogin { get; set; } = false;

        [StringLength(256)]
        public string? PasswordResetToken { get; set; }
    }
}

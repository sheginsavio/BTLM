using System.ComponentModel.DataAnnotations;

namespace Bank_Transaction_and_Loan_management_System.Models.DTOs
{
    public class UserLoginDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}

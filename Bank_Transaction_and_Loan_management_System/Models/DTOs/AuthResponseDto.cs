namespace Bank_Transaction_and_Loan_management_System.Models.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}

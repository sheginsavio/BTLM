using Bank_Transaction_and_Loan_management_System.Models.DTOs;

namespace Bank_Transaction_and_Loan_management_System.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto);
        Task<AuthResponseDto> LoginAsync(UserLoginDto dto);
    }
}

using Bank_Transaction_and_Loan_management_System.Models.DTOs;

namespace Bank_Transaction_and_Loan_management_System.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> UpdateUserRoleAsync(int id, string role);
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}

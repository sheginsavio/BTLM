using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bank_Transaction_and_Loan_management_System.Interfaces;
using Bank_Transaction_and_Loan_management_System.Models.DTOs;

namespace Bank_Transaction_and_Loan_management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            _logger.LogInformation("Registration attempt for username: {Username}", dto.Username);
            var result = await _authService.RegisterAsync(dto);
            _logger.LogInformation("Registration successful for username: {Username}", dto.Username);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            _logger.LogInformation("Login attempt for username: {Username}", dto.Username);
            var result = await _authService.LoginAsync(dto);
            _logger.LogInformation("Login successful for username: {Username}", dto.Username);
            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using UserService.Helper;
using UserService.Services;
using UserService.Userdto;

namespace UserService.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _service;
        private readonly JwtToken _jwt;
        private readonly EmailService _emailService;


        public UserController(IUserServices service, JwtToken jwt,EmailService emailService)
        {
            _service = service;
            _jwt = jwt;
            _emailService = emailService;   
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(RegisterDto dto)
        {
            try
            {
                var user = await _service.Register(dto.FullName, dto.Email, dto.Password);
                string subject = "Welcome";
                string message = "This is our application";
                _emailService.SendEmailAsync(dto.Email, subject, message);
                
  
                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _service.ValidateLogin(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot(ForgotPasswordDto dto)
        {
            var token = await _service.GenerateForgotToken(dto.Email);
            if (token == null)
                return NotFound("Email not found");

            return Ok(new { resetToken = token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset(ResetPasswordDto dto)
        {
            var result = await _service.ResetPassword(dto.Email, dto.ResetToken, dto.NewPassword);
            if (!result)
                return BadRequest("Invalid or expired token");

            return Ok("Password reset successful");
        }
    }
}

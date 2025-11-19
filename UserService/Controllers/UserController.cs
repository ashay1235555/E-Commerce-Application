using Microsoft.AspNetCore.Mvc;
using UserService.Helper;
using UserService.Messaging;
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
        private readonly RabbitMqPublisher _publisher;

        public UserController(IUserServices service, JwtToken jwt, RabbitMqPublisher publisher)
        {
            _service = service;
            _jwt = jwt;
            _publisher = publisher;
        }

        // ========================= SIGN UP (Send OTP) =========================
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(RegisterDto dto)
        {
            try
            {
                var user = await _service.Register(dto.FullName, dto.Email, dto.Password);

                // Publish RabbitMQ event
                _publisher.PublishEvent("user.registered", new
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow
                });

                return Ok("OTP sent to your email. Please verify to activate your account.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ===================== VERIFY ACCOUNT WITH OTP =======================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var success = await _service.VerifyOtp(email, otp);

            if (!success)
                return BadRequest("Invalid or expired OTP!");

            // Publish Event
            _publisher.PublishEvent("user.verified", new
            {
                Email = email,
                VerifiedAt = DateTime.UtcNow
            });

            return Ok("Account verified successfully! You can now login.");
        }

        // ===================== REQUEST OTP FOR LOGIN =========================
        [HttpPost("request-login-otp")]
        public async Task<IActionResult> RequestLoginOtp(string email)
        {
            var result = await _service.SendLoginOtp(email);

            if (!result)
                return BadRequest("Account not found or not verified!");

            _publisher.PublishEvent("user.login.otp.sent", new
            {
                Email = email,
                SentAt = DateTime.UtcNow
            });

            return Ok("Login OTP sent to your email.");
        }

        // ===================== LOGIN WITH OTP + JWT ==========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string otp)
        {
            var user = await _service.ValidateLogin(email, otp);
            if (user == null)
                return Unauthorized("Invalid OTP or expired!");

            var token = _jwt.GenerateToken(user);

            // Publish login event
            _publisher.PublishEvent("user.loggedin", new
            {
                UserId = user.Id,
                Email = user.Email,
                LoggedInAt = DateTime.UtcNow
            });

            return Ok(new { Message = "Login successful", Token = token });
        }

        // ========================= FORGOT PASSWORD =========================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var token = await _service.GenerateForgotToken(dto.Email);
            if (token == null)
                return NotFound("Email not found!");

            _publisher.PublishEvent("user.forgotpassword.requested", new
            {
                Email = dto.Email,
                RequestedAt = DateTime.UtcNow
            });

            return Ok("OTP sent to your email for password reset.");
        }

        // ========================= RESET PASSWORD =========================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var success = await _service.ResetPassword(dto.Email, dto.ResetToken, dto.NewPassword);
            if (!success)
                return BadRequest("Invalid or expired OTP!");

            return Ok("Password reset successful!");
        }

        // ========================= UPLOAD PROFILE IMAGE =====================
        [HttpPost("upload-profile-image")]
        public async Task<IActionResult> UploadProfileImage(int userId, IFormFile file)
        {
            var imageUrl = await _service.UploadProfileImage(userId, file);

            if (imageUrl == null)
                return BadRequest("Image upload failed!");

            _publisher.PublishEvent("user.profile.updated", new
            {
                UserId = userId,
                ImageUrl = imageUrl,
                UpdatedAt = DateTime.UtcNow
            });

            return Ok(new { Message = "Profile updated successfully!", ImageUrl = imageUrl });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using UserService.Context;
using UserService.Entity;
using UserService.Messaging;

namespace UserService.Services
{
    public class UserServices : IUserServices
    {
        private readonly UserContext _db;
        private readonly IEmailService _email;
        private readonly RabbitMqPublisher _publisher;

        public UserServices(UserContext db, IEmailService email, RabbitMqPublisher publisher)
        {
            _db = db;
            _email = email;
            _publisher = publisher;
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // ================= SIGNUP =================
        public async Task<User> Register(string name, string email, string password)
        {
            if (await _db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
                throw new Exception("Email already registered");

            var otp = GenerateOtp();

            var user = new User
            {
                FullName = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                EmailOtp = otp,
                OtpExpiry = DateTime.UtcNow.AddMinutes(10),
                IsVerified = false
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            await _email.SendEmail(email, "Verify Your Account", $"Your OTP is: <b>{otp}</b>");

            _publisher.PublishEvent("user.registered", new
            {
                UserId = user.Id,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow
            });

            return user;
        }

        // ================ VERIFY SIGNUP OTP =================
        public async Task<bool> VerifyOtp(string email, string otp)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || user.EmailOtp != otp || user.OtpExpiry < DateTime.UtcNow)
                return false;

            user.EmailOtp = null;
            user.OtpExpiry = null;
            user.IsVerified = true;
            await _db.SaveChangesAsync();

            await _email.SendEmail(
                email,
                "Welcome to E-Commerce App 🎉",
                $@"
                <div style='font-family: Arial; padding: 20px;'>
                    <h2 style='color:#4CAF50;'>Welcome, {user.FullName}! 🎉</h2>
                    <p>Your account has been successfully verified.</p>
                    <p>We're excited to have you with us!</p>
                </div>"
            );

            _publisher.PublishEvent("user.welcome.sent", new
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Time = DateTime.UtcNow
            });

            return true;
        }

        // ================ REQUEST LOGIN OTP (Email + Password Required) =================
        public async Task<bool> RequestLoginOtp(string email, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || !user.IsVerified)
                return false;

            // Validate password first
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return false;

            var otp = GenerateOtp();

            user.EmailOtp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            await _email.SendEmail(email, "Login OTP", $"Your OTP is: <b>{otp}</b>");

            _publisher.PublishEvent("user.login.otp.sent", new
            {
                Email = user.Email,
                SentAt = DateTime.UtcNow
            });

            return true;
        }

        // ================ LOGIN WITH PASSWORD + OTP =================
        public async Task<User?> LoginWithOtp(string email, string password, string otp)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || !user.IsVerified)
                return null;

            // Check password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // Check OTP
            if (user.EmailOtp != otp || user.OtpExpiry < DateTime.UtcNow)
                return null;

            // Clear OTP
            user.EmailOtp = null;
            user.OtpExpiry = null;
            await _db.SaveChangesAsync();

            _publisher.PublishEvent("user.loggedin", new
            {
                UserId = user.Id,
                Email = user.Email,
                LoggedInAt = DateTime.UtcNow
            });

            return user;
        }

        // ================= FORGOT PASSWORD ==================
        public async Task<string?> GenerateForgotToken(string email)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) return null;

            user.VerificationToken = Guid.NewGuid().ToString("N");
            user.VerificationExpiry = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            await _email.SendEmail(email, "Reset Password", $"Your reset token: <b>{user.VerificationToken}</b>");

            return user.VerificationToken;
        }

        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || user.VerificationToken != token || user.VerificationExpiry < DateTime.UtcNow)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.VerificationToken = null;
            user.VerificationExpiry = null;

            await _db.SaveChangesAsync();
            return true;
        }

        // ================= PROFILE IMAGE ==================
        public async Task<string?> UploadProfileImage(int userId, IFormFile file)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            user.ProfileImageUrl = $"Uploads/{fileName}";
            await _db.SaveChangesAsync();

            return user.ProfileImageUrl;
        }

        // ================ RESEND SIGNUP OTP ==================
        public async Task<bool> ResendOtp(string email)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null) return false;
            if (user.IsVerified) return false;

            var otp = GenerateOtp();

            user.EmailOtp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _db.SaveChangesAsync();

            await _email.SendEmail(
                email,
                "Resend OTP",
                $"Your OTP is: <b>{otp}</b>"
            );

            _publisher.PublishEvent("user.otp.resend", new
            {
                Email = email,
                SentAt = DateTime.UtcNow
            });

            return true;
        }
    }
}

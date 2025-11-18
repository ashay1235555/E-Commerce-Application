using Microsoft.EntityFrameworkCore;
using UserService.Context;
using UserService.Entity;
using UserService.Migrations;

namespace UserService.Services
{
    public class UserServices : IUserServices
    {
        private readonly UserContext _db;
        //private readonly RabbitMqPublisher _publisher;
        //private readonly EmailService _emailService;

        public UserServices(UserContext db)
        {
            _db = db;
            //_emailService = emailService;
        }

        public async Task<User> Register(string name, string email, string password)
        {
            // Email validation case-insensitive
            if (await _db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();


            return user;
        }
        //public async Task<User> Register(string name, string email, string password)
        //{
        //    if (await _db.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
        //        throw new Exception("Email already registered");

        //    var user = new User
        //    {
        //        FullName = name,
        //        Email = email,
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        //    };

        //    await _db.Users.AddAsync(user);
        //    await _db.SaveChangesAsync();

        //    await _emailService.SendWelcomeEmail(user.Email, user.FullName);
        //    return user;
        //}

        public async Task<User?> ValidateLogin(string email, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<string?> GenerateForgotToken(string email)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) return null;

            user.ResetToken = Guid.NewGuid().ToString("N");
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);

            await _db.SaveChangesAsync();
            return user.ResetToken;
        }

        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || user.ResetToken != token || user.ResetTokenExpiry < DateTime.UtcNow)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _db.SaveChangesAsync();
            return true;
        }

        //public Task SendWelcomeEmail(string email, string name)
        //{
        //    return ((IUserServices)_emailService).SendWelcomeEmail(email, name);
        //}
    }
}

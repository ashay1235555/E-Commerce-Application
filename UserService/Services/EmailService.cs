//using MailKit.Net.Smtp;
//using MimeKit;
//using UserService.Entity;
//namespace UserService.Services
//{


//    public class EmailService : IUserServices
//    {
//        private readonly IConfiguration _config;
//        public EmailService(IConfiguration config) => _config = config;

//        public Task<string?> GenerateForgotToken(string email)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<User> Register(string name, string email, string password)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<bool> ResetPassword(string email, string token, string newPassword)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task SendWelcomeEmail(string email, string name)
//        {
//            var msg = new MimeMessage();
//            msg.From.Add(new MailboxAddress(_config["EmailSettings:FromName"], _config["EmailSettings:FromEmail"]));
//            msg.To.Add(new MailboxAddress(name, email));
//            msg.Subject = "Welcome 🎉";

//            msg.Body = new TextPart("html")
//            {
//                Text = $"<h2>Hello {name},</h2> <p>Welcome to our platform!</p>"
//            };

//            using var smtp = new SmtpClient();
//            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
//            await smtp.AuthenticateAsync(_config["EmailSettings:UserName"], _config["EmailSettings:Password"]);
//            await smtp.SendAsync(msg);
//            await smtp.DisconnectAsync(true);
//        }

//        public Task<User?> ValidateLogin(string email, string password)
//        {
//            throw new NotImplementedException();
//        }
//    }

//}


//using MimeKit;
//using MailKit.Net.Smtp;
//using Microsoft.Extensions.Configuration;
//using System.Threading.Tasks;
//namespace UserService.Services
//{
//    public class EmailService
//    {
//        private readonly IConfiguration _configuration;
//        public EmailService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }
//        public async Task SendEmailAsync(string toEmail, string subject, string message)
//        {
//            var emailMessage = new MimeMessage();

//            emailMessage.From.Add(new MailboxAddress(
//                _configuration["EmailSettings:DisplayName"],
//                _configuration["EmailSettings:From"]));

//            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
//            emailMessage.Subject = subject;
//            emailMessage.Body = new TextPart("html") { Text = message };

//            using var client = new SmtpClient();
//            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

//            await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"],
//                                      int.Parse(_configuration["EmailSettings:Port"]),
//                                      MailKit.Security.SecureSocketOptions.StartTls);

//            await client.AuthenticateAsync(_configuration["EmailSettings:From"],
//                                           _configuration["EmailSettings:Password"]);

//            await client.SendAsync(emailMessage);
//            await client.DisconnectAsync(true);
//        }

//    }
//}
using MailKit.Net.Smtp;
using MimeKit;

namespace UserService.Services
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("E-Commerce App", _config["EmailSettings:From"]));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["EmailSettings:UserName"],
                _config["EmailSettings:Password"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}

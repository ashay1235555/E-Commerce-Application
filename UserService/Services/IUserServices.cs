using UserService.Entity;

namespace UserService.Services
{
    public interface IUserServices
    {
        Task<User> Register(string name, string email, string password);
        Task<User?> ValidateLogin(string email, string password);
        Task<string?> GenerateForgotToken(string email);
        Task<bool> ResetPassword(string email, string token, string newPassword);
       
            //Task SendWelcomeEmail(string email, string name);
       

    }
}

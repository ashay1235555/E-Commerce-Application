using UserService.Entity;

namespace UserService.Services
    {
        public interface IUserServices
        {
            Task<User> Register(string name, string email, string password);
            Task<bool> VerifyOtp(string email, string otp);
            //Task<bool> SendLoginOtp(string email);
            //Task<User?> ValidateLogin(string email, string otp);

            Task<string?> GenerateForgotToken(string email);
            Task<bool> ResetPassword(string email, string token, string newPassword);

            Task<string?> UploadProfileImage(int userId, IFormFile file); // optional
            Task<bool> ResendOtp(string email);
            Task<bool> RequestLoginOtp(string email, string password);
            Task<User?> LoginWithOtp(string email, string password, string otp);


    }
}

   


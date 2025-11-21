namespace UserService.Userdto
{
    
        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

    public class LoginOtpDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginVerifyDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Otp { get; set; }
    }

}



namespace UserService.Userdto
{
        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string ResetToken { get; set; }
            public string NewPassword { get; set; }
        }
    }



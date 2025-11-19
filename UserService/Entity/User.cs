namespace UserService.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public DateTime? VerificationExpiry { get; set; }

        public string? ProfileImageUrl { get; set; }
        public string? EmailOtp { get; set; }
        public DateTime? OtpExpiry { get; set; }

    }
}

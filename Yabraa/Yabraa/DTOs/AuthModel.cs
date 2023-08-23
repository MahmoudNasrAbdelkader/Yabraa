namespace Yabraa.DTOs
{
    public class AuthModel
    {
        public string? Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string PhoneNumber { get; set; }
        //public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; internal set; }

        //[JsonIgnore]
        //public string? RefreshToken { get; set; }

        //public DateTime RefreshTokenExpiration { get; set; }
    }
}

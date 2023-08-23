using Org.BouncyCastle.Utilities.IO.Pem;

namespace Yabraa.DTOs
{
    public class ForgotPasswordResponseDto
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string VerificationCode { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Yabraa.DTOs
{
    public class ForgotPasswordDto
    {
        //[Required]
        //[EmailAddress]
        //public string? Email { get; set; }
        //[Required]
        //public string? ClientURI { get; set; }
        public string PhoneNumber { get; set; }
    }
   
}

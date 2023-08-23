using System.ComponentModel.DataAnnotations;

namespace Yabraa.DTOs
{
    public class TokenRequestModel
    {       
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}

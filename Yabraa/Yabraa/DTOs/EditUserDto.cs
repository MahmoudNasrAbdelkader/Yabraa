using System.ComponentModel.DataAnnotations;

namespace Yabraa.DTOs
{
    public class EditUserDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters long.")]
        public string FirstName { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters long.")]
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string CountryCode { get; set; }
        public string? IdOrPassport { get; set; }
        
    }
}

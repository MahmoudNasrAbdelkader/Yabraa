using System.ComponentModel.DataAnnotations;
namespace Yabraa.DTOs
{
    public class RegisterModel
    {
        
        public string? Email { get; set; }
        // [StringLength(maximumLength: 11, ErrorMessage = "The mobile number value cannot exceed 11 characters.",MinimumLength =11)]
        public string PhoneNumber { get; set; }
        //[RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=!*])(?=\\S+$).{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain at least one digit, one lowercase letter, one uppercase letter, and one non-alphanumeric character.")]
        //[StringLength(6, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
        //[StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 3 characters long.")]
        public string FirstName { get; set; }
        //[StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 3 characters long.")]
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string CountryCode { get; set; }
        public string? IdOrIqamaOrPassport { get; set; }
        public string? VerificationCode { get; set; }
    }
}

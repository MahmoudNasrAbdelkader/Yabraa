using System.ComponentModel.DataAnnotations;

namespace Yabraa.DTOs
{
    public class ResetPasswordDto
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string? VerificationCode { get; set; }
        public string Password { get; set; }      
        public string? ConfirmPassword { get; set; }
    }
    //public class ResetPasswordDto
    //{
    //    [Required(ErrorMessage = "Password is required")]
    //    public string? Password { get; set; }
    //    [Required]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string? ConfirmPassword { get; set; }
    //    [Required]
    //    public string? Email { get; set; }
    //    [Required]
    //    public string? Token { get; set; }
    //}
}

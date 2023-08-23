using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAdmin.WebUI.ViewModel
{
    public class EditUserViewModel
    {

        public string Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage =
            "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool Requester { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public int Gender { get; set; }
        public string? IdOrPassport { get; set; }
        [Required]
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }


        // public bool Approver { get; set; }
    }
}

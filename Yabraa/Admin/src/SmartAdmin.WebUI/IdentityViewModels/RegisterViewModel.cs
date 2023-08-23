using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAdmin.WebUI.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
       
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public int Gender { get; set; }
        public string? IdOrPassport { get; set; }
        [Required]
        public string CountryCode { get; set; }
        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password",
        //    ErrorMessage = "Password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }

        [Required]

        public string RoleName { get; set; }

        public bool Requester { get; set; }

        //  public bool Approver { get; set; }

    }
}

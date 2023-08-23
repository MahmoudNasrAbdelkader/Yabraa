using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.ViewModel
{
    public class UserEditViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }

       // [StringLength(6, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
      //  public string? Password { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters long.")]
        public string FirstName { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters long.")]
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; }
        public string CountryCode { get; set; }
        public string? IdOrPassport { get; set; }
        public SelectList Countries { get; set; }

    }
}

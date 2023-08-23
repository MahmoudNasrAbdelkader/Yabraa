using System;
using System.Collections.Generic;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.ViewModel
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }   
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int Gender { get; set; }
        public string IdOrPassport { get; set; }
        public string Status { get; set; }
        public  Country Country { get; set; }
        public DateTime CrateDateTime { get; set; }
        public List<UserFamily> Family { get; set; }
        public List<VisitIndexViewModel> Visits { get; set; }
    }
}

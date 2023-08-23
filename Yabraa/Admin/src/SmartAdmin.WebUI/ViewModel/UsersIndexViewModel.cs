using Microsoft.AspNetCore.Identity;

namespace SmartAdmin.WebUI.ViewModel
{
    public class UsersIndexViewModel
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
    }
}

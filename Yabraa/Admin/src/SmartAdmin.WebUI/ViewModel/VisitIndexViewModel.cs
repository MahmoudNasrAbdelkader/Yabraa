using System;

namespace SmartAdmin.WebUI.ViewModel
{
    public class VisitIndexViewModel
    {
        public long VisitDetailsId { get; set; }
        public DateTime VisitDT { get; set; }
        public string ApplicationUserName { get; set; }
        public string ApplicationUserId { get; set; }
        public string UserFamilyName { get; set; }
        public string PackageName { get; set; }
        public int PackageId { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public bool payment { get; set; }
        public string Type { get; set; }
        public DateTime CurrentDT { get; set; }
    }
}

using System;
using System.Collections.Generic;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.ViewModel
{
    public class VisitViewModel
    {
        public long VisitDetailsId { get; set; }
        public DateTime VisitDT { get; set; }
        public double LocationLongitude { get; set; }
        public double LocationLatitude { get; set; }
        public double? LocationAltitude { get; set; }
        public string Notes { get; set; }
        public string ApplicationUserName { get; set; }
        public string ApplicationUserId { get; set; }
        public string UserFamilyName { get; set; }
        public string UserFamilyGender { get; set; }
        public DateTime UserFamilyBirthDate { get; set; }
        public string PackageNameAR { get; set; }
        public string PackageNameEN { get; set; }
        public int PackageId { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string ServiceType { get; set; }
        public bool payment { get; set; }
        public List<VisitNotes> visitNotes { get; set; }
        public List<VisitAttachments>  visitAttachments { get; set; }


    }
}

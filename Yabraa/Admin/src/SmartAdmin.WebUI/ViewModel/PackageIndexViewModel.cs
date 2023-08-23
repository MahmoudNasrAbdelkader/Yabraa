using System.ComponentModel.DataAnnotations.Schema;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.ViewModel
{
    public class PackageIndexViewModel
    {
        public int PackageId { get; set; }
        public string NameAR { get; set; }
        public string NameEN { get; set; }
        public string Service { get; set; }
        public string Category { get; set; }
        public string ServiceType { get; set; }

    }
}

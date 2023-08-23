using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartAdmin.WebUI.ViewModel
{
    public class ServicesCreateViewModel
    {
        public int? ServiceId { get; set; }
        public int ServiceTypeId { get; set; }
        public List<YabraaEF.Models.Service> ChildService { get; set; }
        [Required]
        public string NameAR { get; set; }
        public string NameEN { get; set; }
        public string DetailsAR { get; set; }
        public string DetailsEN { get; set; }
        //public int ParentServiceId { get; set; }
        public IFormFile File { get; set; }
        public string Path { get; set; }
    }
}

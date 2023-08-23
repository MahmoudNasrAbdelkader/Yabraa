using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAdmin.WebUI.ViewModel
{
    public class PackageCreateViewModel
    {
        public SelectList Services { get; set; }
        public int? PackageId { get; set; }
        [Required]
        public string NameAR { get; set; }
        public string NameEN { get; set; }
        public string SubTitleAR { get; set; }
        public string SubTitleEN { get; set; }
        public string DetailsAR { get; set; }
        public string DetailsEN { get; set; }
        public string InstructionAR { get; set; }
        public string InstructionEN { get; set; }
        [Required]
        public decimal? Price { get; set; }
        public IFormFile File { get; set; }
        [Required]
        public int? ServiceId { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        public string Path { get; set; }
    }
}

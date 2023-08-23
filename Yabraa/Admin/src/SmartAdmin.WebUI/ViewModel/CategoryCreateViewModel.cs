using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAdmin.WebUI.ViewModel
{
    public class CategoryCreateViewModel
    {
        public int? CategoryId { get; set; }
        [Required]
        public string NameAR { get; set; }
        public string NameEN { get; set; }
        [Required]
        public int? ServiceId { get; set; }
        public SelectList Services { get; set; }
    }
}

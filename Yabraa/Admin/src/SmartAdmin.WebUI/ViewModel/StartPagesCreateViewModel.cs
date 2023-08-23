using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartAdmin.WebUI.ViewModel
{
    public class StartPagesCreateViewModel
    {
        public int? StartPageId { get; set; }
        [Required]
        public string TitleEn { get; set; }
        [Required]
        public string SubTitleEn { get; set; }
        [Required]
        public string TitleAr { get; set; }
        [Required]
        public string SubTitleAR { get; set; }       
        public string Path { get; set; }
        public IFormFile File { get; set; }
    }
}

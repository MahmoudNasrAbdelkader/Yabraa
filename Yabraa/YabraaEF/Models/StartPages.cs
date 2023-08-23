using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class StartPages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StartPageId { get; set; }
        public string TitleEn { get; set; }
        public string SubTitleEn { get; set; }
        public string TitleAr { get; set; }
        public string SubTitleAR { get; set; }
        public int? OrderbyAscending { get; set; }
        public string Path { get; set; }
    }
}

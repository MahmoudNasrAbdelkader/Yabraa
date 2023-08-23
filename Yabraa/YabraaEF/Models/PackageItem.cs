using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class PackageItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageItemId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public int PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }

    }

}

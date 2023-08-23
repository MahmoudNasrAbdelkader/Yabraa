using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class Package
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? SubTitleAR { get; set; }
        public string? SubTitleEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public string? InstructionAR { get; set; }
        public string? InstructionEN { get; set; }      
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        public DateTime CreateDT { get; set; }
        public string CreateSystemUserId { get; set; }
        public bool Deleted { get; set; }
    }
}

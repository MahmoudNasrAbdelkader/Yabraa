using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace YabraaEF.Models
{
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public string? ImagePath { get; set; }
        public int? ParentServiceId { get; set; }
        public DateTime CreateDT { get; set; }
        public string CreateSystemUserId { get; set; }
        public bool Deleted { get; set; }
        public int ServiceTypeId { get; set; }
        [ForeignKey("ServiceTypeId")]
        public virtual ServiceType ServiceType { get; set; }
        public virtual ICollection<Package> Packages { get; }
        public virtual ICollection<Category> Categories { get; }
    }

}

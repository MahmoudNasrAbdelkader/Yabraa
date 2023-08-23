using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class ServiceType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enable { get; set; }
        public DateTime CreateDT { get; set; }
        public virtual ICollection<Service> Services { get; }
        public virtual ICollection<VisitDetails> VisitDetails { get; }

    }
}

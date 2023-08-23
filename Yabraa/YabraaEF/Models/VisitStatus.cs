using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class VisitStatus
    {
        [Key]
     
        public int VisitStatusId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Deleted { get; set; }
        public virtual ICollection<VisitDetails> VisitDetails { get; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class VisitNotes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long VisitNoteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreateDTs { get; set; }
        public string CreateSystemUserId { get; set; }
        public long  VisitDetailsId { get; set; }
        [ForeignKey("VisitDetailsId")]
        public virtual VisitDetails VisitDetails { get; set; }
    }
}

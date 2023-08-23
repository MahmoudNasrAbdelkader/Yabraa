using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class VisitAttachments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long VisitAttachmentId { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public DateTime CreateDTs { get; set; }
        public string CreateSystemUserId { get; set; }
        public long VisitDetailsId { get; set; }
        [ForeignKey("VisitDetailsId")]
        public virtual VisitDetails VisitDetails { get; set; }
    }
}

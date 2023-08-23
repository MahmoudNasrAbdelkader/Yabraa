using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class InvoiceDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InvoiceDetailsId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public long InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }
        public int PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        public virtual ICollection<VisitDetails> VisitDetails { get; }

    }
}

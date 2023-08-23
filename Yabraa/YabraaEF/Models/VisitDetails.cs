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
    public class VisitDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long VisitDetailsId { get; set; }
      
        public DateTime VisitDT { get; set; }
        public double? LocationLongitude { get; set; }
        public double? LocationLatitude { get; set; }
        public double? LocationAltitude { get; set; }
        public string? Notes { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public long UserFamilyId { get; set; }
        [ForeignKey("UserFamilyId")]
        public virtual UserFamily UserFamily { get; set; }
        public long InvoiceDetailsId { get; set; }
        [ForeignKey("InvoiceDetailsId")]
        public virtual InvoiceDetails InvoiceDetails { get; set; }
        public int PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        public int? VisitStatusId { get; set; }
        [ForeignKey("VisitStatusId")]    
        public virtual VisitStatus VisitStatus { get; set; }
        public int ServiceTypeId { get; set; }
        [ForeignKey("ServiceTypeId")]
        public virtual ServiceType ServiceType { get; set; }
        public virtual ICollection<VisitAttachments> VisitAttachments { get;  }
        public virtual ICollection<VisitNotes> VisitNotes { get;}


    }
}

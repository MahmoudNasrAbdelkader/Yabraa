using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InvoiceId { get; set; }
        public double? LocationLongitude { get; set; }
        public double? LocationLatitude { get; set; }
        public double? LocationAltitude { get; set; }
        public decimal TotalPrice { get; set; }
        public string? CheckoutId { get; set; }
        public int PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }
        public bool Paid { get; set; }
        public DateTime CreateDT { get; set; }
        public string UserId { get; set; }
        public virtual ICollection<InvoiceDetails> InvoiceDetails { get; }

    }
}

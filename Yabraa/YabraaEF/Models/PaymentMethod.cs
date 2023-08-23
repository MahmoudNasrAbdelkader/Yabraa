using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class PaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public DateTime CreateDT { get; set; }
    }
}

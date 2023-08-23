using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class Country
    {
        [Key]
        [Column(TypeName = "varchar(2)")]
        public string CountryCode { get; set; }
        public string CountryEnName { get; set; }
        public string CountryArName { get; set; }
        public string CountryEnNationality { get; set; }
        public string CountryArNationality { get; set; }

    }
}

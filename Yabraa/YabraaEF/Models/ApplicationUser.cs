using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; }
        public string? IdOrPassport { get; set; }
       // [Column(TypeName = "varchar(2)")]
        public string CountryCode { get; set; }
        [ForeignKey("CountryCode")]
        public virtual Country Country { get; set; }
        public int? verificationCode { get; set; }
        public DateTime CrateDateTime { get; set; }

        //public List<RefreshToken>? RefreshTokens { get; set; }
        public bool IsSystemUser { get; set; }

        public bool Deleted { get; set; }

    }

}

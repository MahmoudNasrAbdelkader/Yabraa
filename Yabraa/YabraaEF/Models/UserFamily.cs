using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class UserFamily
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserFamilyId { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        //public int? FamilyRelationshipId { get; set; }
        //[ForeignKey("FamilyRelationshipId")]
        //public virtual FamilyRelationship FamilyRelationship { get; set; }
        public string ApplicationUserId { get; set; }
        public int Gender { get; set; }
        public bool Deleted { get; set; }
        public virtual ICollection<VisitDetails> VisitDetails { get; }
        public bool IsOwner { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Models
{
    public class Gallery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GalleryId { get; set; }
       // [Index(IsUnique = true)]
        public int OrderbyAscending { get; set; }
        public string Path { get; set; }
    }
}

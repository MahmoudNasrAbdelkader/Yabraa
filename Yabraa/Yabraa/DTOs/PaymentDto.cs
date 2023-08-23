using Microsoft.Build.Framework;

namespace Yabraa.DTOs
{
    public class PaymentDto
    {
        public double locationLongitude { get; set; }
        public double locationLatitude { get; set; }
        public double? locationAltitude { get; set; }
        //public decimal totalPrice { get; set; }
        [Required]
        public decimal amount { get; set; }
        [Required]
        public string currency { get; set; }
        [Required]
        public int PaymentMethodId { get; set; }
        public int ServiceTypeId { get; set; }
        public List<PaymentPackageDto> packages { get; set; }
    }
}

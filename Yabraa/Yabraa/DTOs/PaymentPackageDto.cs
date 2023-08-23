namespace Yabraa.DTOs
{
    public class PaymentPackageDto
    {
        public int packageId { get; set; }
        public long userFamilyId { get; set; }       
        public string? notes { get; set; }
        public decimal price { get; set; }
        public DateTime dateTime { get; set; }
    }
}

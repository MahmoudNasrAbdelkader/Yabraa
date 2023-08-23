namespace Yabraa.DTOs
{
    public class AppointmentDto
    {
        public string PackageNameAR { get; set; }
        public string? PackageNameEN { get; set; }
        public string ServiceAR { get; set; }
        public string? ServiceEN { get; set; }
        public DateTime VisitDT { get; set; }
        public TimeSpan VisitTime { get; set; }
        public decimal Price { get; set; }
        public string UserFamilyName { get; set; }
        public string Status { get; set; }
        public long AppointmentId { get; set; }

    }
}

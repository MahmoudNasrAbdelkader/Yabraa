
namespace Yabraa.DTOs
{
    public class AppointmentDetailsDto
    {
        public long AppointmentId { get; set; }
        public string UserFamilyName { get; set; }
        public double? LocationLongitude { get; set; }
        public double? LocationLatitude { get; set; }
        public double? LocationAltitude { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
        public string PackageNameAR { get; set; }
        public string? PackageNameEN { get; set; }
        public string Status { get; set; }
        public string ServiceAR { get; set; }
        public string? ServiceEN { get; set; }
        public DateTime VisitDT { get; set; }
        public TimeSpan VisitTime { get; set; }
        public List<VisitNoteDTO> VisitNotes { get; set; }
        public List<VisitAttachmentDTO> VisitAttachments { get; set; }
    }
}

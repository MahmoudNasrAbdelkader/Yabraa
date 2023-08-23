using Microsoft.Build.Framework;

namespace Yabraa.DTOs
{
    public class AppointmentEditDto
    {
        public long AppointmentId { get; set; }
        [Required]
        public double LocationLongitude { get; set; }
        [Required]
        public double LocationLatitude { get; set; }
        public double? LocationAltitude { get; set; }
        public string? Notes { get; set; }
    }
}

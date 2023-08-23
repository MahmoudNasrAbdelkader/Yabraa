namespace Yabraa.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public string? ImagePath { get; set; }
        public int? ParentServiceId { get; set; }

        public List<CategoryDto> Filters { get; set; }
    }
}

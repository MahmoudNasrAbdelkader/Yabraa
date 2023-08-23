namespace Yabraa.DTOs
{
    public class CategoryDto
    {
        public int FilterId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public int ServiceId { get; set; }
        public int? ParentServiceId { get; set; }
        public List<PackageDto> Packages { get; set; }

    }
}

namespace Yabraa.DTOs
{
    public class PackageDto
    {
        public int PackageId { get; set; }
        public string NameAR { get; set; }
        public string? NameEN { get; set; }
        public string? SubTitleAR { get; set; }
        public string? SubTitleEN { get; set; }
        public string? DetailsAR { get; set; }
        public string? DetailsEN { get; set; }
        public string? InstructionAR { get; set; }
        public string? InstructionEN { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public int ServiceId { get; set; }
        public int FilterId { get; set; }
    }
}

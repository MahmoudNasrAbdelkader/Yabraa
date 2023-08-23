namespace Yabraa.DTOs
{
    public class HistoryPaymentDto
    {
        public List<HistoryVisitsDTO> MyProperty { get; set; }
    }
    public class HistoryVisitsDTO
    {
        public string PackageNameAR { get; set; }
        public string? PackageNameEN { get; set; }
        public string ServiceAR { get; set; }
        public string? ServiceEN { get; set; }
        public string VisitDT { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
       
    }
}

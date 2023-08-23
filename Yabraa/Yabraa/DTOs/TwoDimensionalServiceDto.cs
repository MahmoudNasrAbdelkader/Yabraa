namespace Yabraa.DTOs
{
    public class TwoDimensionalServiceDto
    {
        public ServiceDto ParentService { get; set; }
        public ServiceDto FirstService { get; set; }
        public ServiceDto SecondService { get; set; }
    }
}

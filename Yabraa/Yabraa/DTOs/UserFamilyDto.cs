namespace Yabraa.DTOs
{
    public class UserFamilyDto
    {
        public long UserFamilyId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
    public class UserFamilyViewDto
    {
        public long UserFamilyId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public bool IsOwner { get; set; }
    }
}

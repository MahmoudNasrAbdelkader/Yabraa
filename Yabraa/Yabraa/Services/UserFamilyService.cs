using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Yabraa.Const;
using Yabraa.DTOs;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Services
{
    public class UserFamilyService
    {
        private ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserFamilyService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;

        }
        public  List<UserFamilyViewDto> GetUserFamily(string userId)
        {
          
            return  _dbContext.UserFamilies.Where(c=>c.ApplicationUserId==userId && !c.Deleted).Select(c => new UserFamilyViewDto { UserFamilyId= c.UserFamilyId,Name=c.Name,BirthDate=c.BirthDate.Date.ToString("yyyy-MM-dd") ,Gender= (c.Gender==0?"Male": "Female"),IsOwner=c.IsOwner }).ToList();
        }
        public List<UserFamilyViewDto> AddUserFamily(string UserId, UserFamilyDto userFamily)
        {
            //var user = await _userManager.FindByNameAsync(UserId);
            UserFamily model = new UserFamily()
            {
                Name = userFamily.Name,
                BirthDate = userFamily.BirthDate,
                Deleted = false,
                ApplicationUserId = UserId, 
                Gender = (userFamily.Gender.Trim().ToLower() == "male" ? (int)Gender.Male : (int)Gender.Female),
                IsOwner = false
            };
            _dbContext.UserFamilies.Add(model);
            _dbContext.SaveChanges();

            var Families = GetUserFamily(UserId); //_dbContext.UserFamilies.Where(c => c.ApplicationUserId == UserId).Select(c => new UserFamilyDto { UserFamilyId = c.UserFamilyId, Name = c.Name, BirthDate = c.BirthDate }).ToList();
            return Families;
        }
        public bool DeleteUserFamily(long UserFamilyId)
        {
           var userFamliy = _dbContext.UserFamilies.FirstOrDefault(c => c.UserFamilyId == UserFamilyId);
            if (userFamliy is not null)
            {
                userFamliy.Deleted = true;
                _dbContext.SaveChanges();
                return true;
            }
            return false;            
        }

        internal UserFamilyViewDto EditUserFamily(UserFamilyDto userFamilyDto ,string userId)
        {
            var userFamliy = _dbContext.UserFamilies.FirstOrDefault(c => c.UserFamilyId == userFamilyDto.UserFamilyId && c.ApplicationUserId == userId);
            if (userFamliy is not null)
            {
                userFamliy.BirthDate = userFamilyDto.BirthDate;
                userFamliy.Gender = (userFamilyDto.Gender.Trim().ToLower() == "male" ? (int)Gender.Male : (int)Gender.Female);
                userFamliy.Name = userFamilyDto.Name;
                _dbContext.SaveChanges();
                return GetUserFamilyById( userId, userFamilyDto.UserFamilyId);
            }
            return null;
        }
        public UserFamilyViewDto GetUserFamilyById(string userId, long UserFamilyId)
        {
            var userFamliy = _dbContext.UserFamilies.FirstOrDefault(c => c.UserFamilyId == UserFamilyId && c.ApplicationUserId == userId);
            if (userFamliy is not null)
            {
                UserFamilyViewDto userFamilyViewDto = new UserFamilyViewDto()
                {
                    BirthDate = userFamliy.BirthDate.Date.ToString("yyyy-MM-dd"),
                    Name = userFamliy.Name,
                    Gender = (userFamliy.Gender == 0 ? "Male" : "Female"),
                    UserFamilyId = userFamliy.UserFamilyId,
                };
                return userFamilyViewDto;
            }
            return null;
        }
        //public async Task<List<AppointmentDto>> GetAppointmentsByUserId(string Id)
        //{
        //    return await _dbContext.VisitDetails.Where(c => c.ApplicationUserId == Id)
        //       .Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package.Service).Include(c => c.InvoiceDetails.Invoice)
        //       .Select(c => new AppointmentDto
        //       {                
        //           PackageNameAR = c.Package.NameAR,
        //           PackageNameEN = c.Package.NameEN,
        //           ServiceAR = c.Package.Service.NameAR,
        //           ServiceEN = c.Package.Service.NameEN,
        //           Price = c.InvoiceDetails.Price,
        //           UserFamilyName = c.UserFamily.Name,
        //           VisitDT = c.VisitDT,
        //           Status = c.VisitStatus.Name,
        //           VisitTime = c.VisitDT.TimeOfDay,
        //       }).ToListAsync();
           
        //}
    }
}

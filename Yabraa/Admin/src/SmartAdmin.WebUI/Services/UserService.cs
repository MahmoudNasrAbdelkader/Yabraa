using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Consts;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Services
{
    public class UserService
    {
        public ApplicationDbContext _context { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public VisitsService _visitsService { get; set; }
        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, VisitsService visitsService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _visitsService = visitsService;
        }

        public async Task<PagingViewModel<UsersIndexViewModel>> getUsersMobile(int currentPage, string Search = null)
        {
            PagingViewModel<UsersIndexViewModel> model = new PagingViewModel<UsersIndexViewModel>();
            var users = _context.Users.Where(c =>
            (!string.IsNullOrWhiteSpace(Search) ? (c.UserName.Contains(Search) || c.PhoneNumber.Contains(Search) || c.LastName.Contains(Search) || c.FirstName.Contains(Search)) : true)
            && !c.IsSystemUser

            ).Skip((currentPage - 1) * TablesMaxRows.UserMobileIndex).Take(TablesMaxRows.UserMobileIndex).Select(c => new UsersIndexViewModel() { Id = c.Id, PhoneNumber = c.UserName, FirstName = c.FirstName, LastName = c.LastName ,Status = c.Deleted ? "Deleted" : "Active" }).ToListAsync();



            model.items = await users;
            var itemsCount = _context.Users.Where(c =>
            (!string.IsNullOrWhiteSpace(Search) ? (c.UserName.Contains(Search) || c.PhoneNumber.Contains(Search) || c.LastName.Contains(Search) || c.FirstName.Contains(Search)) : true)
            && !c.IsSystemUser

            ).Count();

            double pageCount =  (double)(itemsCount / Convert.ToDecimal(TablesMaxRows.UserMobileIndex));
            model.PageCount = (int)Math.Ceiling(pageCount);
            model.CurrentPageIndex = currentPage;
            model.itemsCount = itemsCount;
            model.Tablelength = TablesMaxRows.UserMobileIndex;
            return model;
        }
        public Task<PagingViewModel<UsersIndexViewModel>>  getAllPagingWithChangelength(int currentPageIndex, int length)
        {
            TablesMaxRows.UserMobileIndex = length;
            return getUsersMobile(currentPageIndex);
        }
        public async Task<UserViewModel> getUserDetails(string id)
        {
            //var user = await _userManager.FindByIdAsync(id);
            var user = await _context.Users.Include(u => u.Country).FirstOrDefaultAsync(u => u.Id == id);
            if (user is not null)
            {
                UserViewModel userViewModel = new UserViewModel()
                {
                    BirthDate = user.BirthDate,
                    Country = user.Country,
                    CrateDateTime = user.CrateDateTime,
                    FirstName = user.FirstName,
                    IdOrPassport = user.IdOrPassport,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    UserName = user.UserName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Family = _context.UserFamilies.Where(c => c.ApplicationUserId == id && !c.Deleted).ToList(),
                    Visits = await _visitsService.GetVisitsBYUserId(id),
                    Status = user.Deleted ? "Deleted" : "Active"
                };
                return userViewModel;
            }
            return null;
        }
        public async Task<UserEditViewModel> GetUserApplicationById(string id)
        {
            
            var user = await _context.Users.Include(u => u.Country).FirstOrDefaultAsync(u => u.Id == id);
            if (user is not null)
            {
                UserEditViewModel userViewModel = new UserEditViewModel()
                {
                    BirthDate = user.BirthDate,
                    CountryCode = user.Country.CountryCode,                    
                    FirstName = user.FirstName,
                    IdOrPassport = user.IdOrPassport,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Username = user.UserName,
                    Id = user.Id,                   
                    Email = user.Email                  
                };
                return userViewModel;
            }
            return null;
        }
        public async Task<SelectList> GetCountries(string CountryCode = null)
        {
        
            var Countries = await _context.Countries.ToListAsync();
            SelectList items;
            if (!string.IsNullOrWhiteSpace(CountryCode))
                items = new SelectList(Countries, "CountryCode", "CountryArNationality", CountryCode);
            else
                items = new SelectList(Countries, "CountryCode", "CountryArNationality");

            return items;
        }
        public async Task<string> EditUserApplication(UserEditViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == model.Id);

            if (user is not null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.BirthDate = model.BirthDate;
                user.CountryCode = model.CountryCode;
                user.Email = model.Email;
                user.Gender = model.Gender;
                user.IdOrPassport = model.IdOrPassport;
                await _context.SaveChangesAsync();
                return user.Id;
            }
            return null;

        }
    }
}

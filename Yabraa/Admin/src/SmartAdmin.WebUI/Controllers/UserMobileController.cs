using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Services;
using SmartAdmin.WebUI.ViewModel;
using X.PagedList;
using YabraaEF;
using YabraaEF.Models;
using static SmartAdmin.WebUI.ViewModel.Permissions;

namespace SmartAdmin.WebUI.Controllers
{

    [Authorize(Permissions.UsersMobile.Manage)]
    public class UserMobileController : Controller
    {
        public SmartAdmin.WebUI.Services.UserService _userService { get; set; }
        public VisitsService _visitsService { get; set; }
        public ApplicationDbContext _context { get; set; }

        public UserMobileController(UserService userService, ApplicationDbContext context, VisitsService visitsService)
        {
            _userService = userService;
            _context = context;
            _visitsService = visitsService;
        }


        public async Task<IActionResult> Index(int CurrentPageIndex = 1, string Search = null)
        {
            try
            {

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    ViewBag.Search = Search;
                }
                var model = await _userService.getUsersMobile(CurrentPageIndex, Search);
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpPost]
        public async Task<IActionResult> Changelength(int length, int CurrentPageIndex = 1)
        {
            try
            {
                return View("Index", await _userService.getAllPagingWithChangelength(CurrentPageIndex, length));
            }
            catch (Exception ex)
            {
                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditUserApplication(string Id)
        {
            try
            {
                var model = await _userService.GetUserApplicationById(Id);
                if (model is not null)
                {
                    model.Countries = await _userService.GetCountries(model.CountryCode);
                    return View("EditUserApplication", model);
                }
                return await View("Error404");
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditUserApplication(UserEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid )
                {
                    var Id = await _userService.EditUserApplication(model);
                    if (Id is not null)
                    {
                        return RedirectToAction("View" , new{ Id });
                    }
                    return await View("Error404");
                }
                model.Countries = await _userService.GetCountries(model.CountryCode);
                return View("Create", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> View(string Id)
        {
            try
            {
                var user = await _userService.getUserDetails(Id);
                if (user is not  null)
                {
                    return  View(user);
                }

                return await View("Error404");
            }
            catch (Exception ex)
            {
                return View("Error404", ex);
            }
        }
    }

}

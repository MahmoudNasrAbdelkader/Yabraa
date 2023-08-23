using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Services;
using YabraaEF.Models;
using static SmartAdmin.WebUI.ViewModel.Permissions;

namespace SmartAdmin.WebUI.Controllers
{
    public class VisitsController : Controller
    {
        public VisitsService _visitsService { get; set; }

        public VisitsController(VisitsService visitsService)
        {
            _visitsService = visitsService;
        }
        public async Task<IActionResult> Normal(string getBy = "daily", int month = 0)
        {
            try
            {
                var model = await _visitsService.GetVisits("Normal", getBy, month);
                if (model is null)
                {
                    return View("Error404");
                }
                ViewBag.ServiceType = "Normal";
                ViewBag.monthNumber = month ;
                ViewBag.CountVisitsDaily = await _visitsService.GetVisitsCount("Normal","daily");
                ViewBag.CountVisitsMonthly = await _visitsService.GetVisitsCount("Normal","Monthly");
                ViewBag.CountVisitsMonth = await _visitsService.GetVisitsCount("Normal","Monthly" , month);
                return View("Index",model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        public async Task<IActionResult> Virtual(string getBy = "daily", int month = 0)
        {
            try
            {
                var model = await _visitsService.GetVisits("Virtual", getBy, month);
                if (model is null)
                {
                    return View("Error404");
                }
                ViewBag.ServiceType = "Virtual";
                ViewBag.monthNumber = month;
                ViewBag.CountVisitsDaily = await _visitsService.GetVisitsCount("Virtual","daily");
                ViewBag.CountVisitsMonthly = await _visitsService.GetVisitsCount("Virtual","Monthly");
                ViewBag.CountVisitsMonth = await _visitsService.GetVisitsCount("Virtual","Monthly", month);
                return View("Index", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> View(int VisitDetailsId,string ErrorMessageNoteOrAttachment = null)
        {
            try
            {
                var model = await _visitsService.GetVisitById(VisitDetailsId);
                if (model is not null)
                {
                    if (!string.IsNullOrWhiteSpace(ErrorMessageNoteOrAttachment))
                    {
                        ViewBag.ErrorMessageNoteOrAttachment = "Error, Please check your inputs";
                    }
                    return View(model);
                }
                return View("Error404");
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        public async Task<IActionResult> ChangeStatus(int VisitDetailsId,string Status)
        {
            try
            {
                var model = await _visitsService.ChangeStatus(VisitDetailsId, Status);
                if (model > 0)
                {
                    return RedirectToAction("View", new { VisitDetailsId });
                }
                return View("Error404");
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        public async Task<IActionResult> Search(string ServiceType, string Status, DateTime From, DateTime? To)
        {
            try
            {
                ViewBag.ServiceType = ServiceType;
                ViewBag.StatusSearch = Status;
                ViewBag.FromSearch = From;
                if (To.HasValue)
                {
                    ViewBag.ToSearch = To;
                }
              
                ViewBag.CountVisitsDaily = await _visitsService.GetVisitsCount(ServiceType,"daily");
                ViewBag.CountVisitsMonthly = await _visitsService.GetVisitsCount(ServiceType,"Monthly");
                ViewBag.CountVisitsMonth = await _visitsService.GetVisitsCount(ServiceType, "Monthly");
                if (To.HasValue && From > To)
                {
                    ViewBag.errorMessageSearch = "To date cannot be younger than From date.";
                    var model2 = await _visitsService.GetVisits(ServiceType);
                    View("Index", model2);
                }
                var model = await _visitsService.Search(ServiceType,Status, From, To);

                return View("Index", model);

            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
          
        }
        [HttpPost]
        public async Task<IActionResult> AddNote(int VisitDetailsId, string Title, string Description)
        {
            try
            {
                if (VisitDetailsId > 0 && !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Description))
                {
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    //var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                    var NoteId = await _visitsService.AddNote(VisitDetailsId, Title, Description, userId);
                    if (NoteId > 0)
                    {
                      
                        return RedirectToAction("View", new { VisitDetailsId });
                    }

                }               
             
                return RedirectToAction("View", new { VisitDetailsId, ErrorMessageNoteOrAttachment = "Error, Please check your inputs" });
              
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddAttachment(int VisitDetailsId, string Title, IFormFile File)
        {
            try
            {
                if (VisitDetailsId > 0 && !string.IsNullOrWhiteSpace(Title) && File != null && File.Length > 0)
                {
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    //var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                    var AttachmentId = await _visitsService.AddAttachment(VisitDetailsId, Title, File, userId);
                    if (AttachmentId > 0)
                    {
                        return RedirectToAction("View", new { VisitDetailsId });
                    }

                }
                return RedirectToAction("View", new { VisitDetailsId, ErrorMessageNoteOrAttachment = "Error, Please check your inputs" });
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }

    }
}

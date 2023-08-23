using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAdmin.WebUI.Services;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Controllers
{
    [Authorize(Permissions.StartPages.Manage)]
    public class StartPagesController : Controller
    {
        public SmartAdmin.WebUI.Services.StartPagesService _startPagesService { get; set; }

        public StartPagesController(StartPagesService startPagesService)
        {
            _startPagesService = startPagesService;
            
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _startPagesService.GetstartPages();
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        public IActionResult Create()
        {                          
                return  View(new StartPagesCreateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(StartPagesCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var Id = await _startPagesService.Create(model);
                    if (Id > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int StartPageId)
        {
            try
            {
                var model = await _startPagesService.GetStartPageById(StartPageId);
                if (model is not null)
                {
                    return View("Create", model);
                }
                return View("Error404");
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(StartPagesCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid && model.StartPageId.HasValue)
                {
                    var StartPageId = await _startPagesService.Edit(model);
                    if (StartPageId is not null)
                    {
                        // return RedirectToAction("View", StartPageId);
                        return RedirectToAction("Index");
                    }
                    return View("Error404");
                }
                return View("Create", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int StartPageId)
        {
            try
            {
                var status = await _startPagesService.Delete(StartPageId);
                if (status)
                {
                    return Json(new {status = 1 ,message = "Done" });
                }
                return Json(new { status = 1, message = "Error" });
            }
            catch (System.Exception ex)
            {
                return Json(new { status = 1, message = ex.Message });
            }
        }

    }
}

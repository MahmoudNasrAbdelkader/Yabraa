using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAdmin.WebUI.Services;
using SmartAdmin.WebUI.ViewModel;

namespace SmartAdmin.WebUI.Controllers
{
    [Authorize(Permissions.Packages.Manage)]
    public class PackageController : Controller
    {
        public SmartAdmin.WebUI.Services.Service _service { get; set; }
        public CategoryService _categoryService { get; set; }
        public PackageService _packageService { get; set; }

        public PackageController(SmartAdmin.WebUI.Services.Service service, CategoryService categoryService, PackageService packageService)
        {
            _service = service;
            _categoryService = categoryService;
            _packageService = packageService;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _packageService.GetPackages();
                return View(model);
            }
            catch (System.Exception ex)
            {
                return View("Error404", ex);
            }
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                PackageCreateViewModel model = new PackageCreateViewModel()
                {
                    Services = await _service.GetServicesWithoutParents()
                };
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpPost]
        public async Task<IActionResult> Create(PackageCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var packageId = await _packageService.Create(model);
                    if (packageId > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                model.Services = await _service.GetServicesWithoutParents();
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int PackageId)
        {
            try
            {
                var model = await _packageService.GetPackageById(PackageId);
                if (model is not null)
                {
                    model.Services = await _service.GetServicesWithoutParents(model.ServiceId.Value);
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
        public async Task<IActionResult> Edit(PackageCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid && model.PackageId.HasValue)
                {
                    var packageId = await _packageService.Edit(model);
                    if (packageId is not null)                    {
                       
                        return RedirectToAction("Index");
                    }
                    return View("Error404");
                }
                model.Services = await _service.GetServicesWithoutParents(model.ServiceId.Value);
                return View("Create", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int PackageId)
        {
            try
            {
                var status = await _packageService.Delete(PackageId);
                if (status)
                {
                    return Json(new { status = 1, message = "Done" });
                }
                return Json(new { status = 1, message = "Error" });
            }
            catch (System.Exception ex)
            {
                return Json(new { status = 1, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> View(int PackageId)
        {
            try
            {
                var model = await _packageService.GetPackageById(PackageId);
                if (model is not null)
                {
                    model.Services = await _service.GetServicesWithoutParents(model.ServiceId.Value);
                    return View("View", model);
                }
                return View("Error404");
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
    }
}

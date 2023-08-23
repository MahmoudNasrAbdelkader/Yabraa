using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAdmin.WebUI.Services;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Controllers
{
    [Authorize(Permissions.Filters.Manage)]
    public class CategoryController : Controller
    {
        public SmartAdmin.WebUI.Services.Service _service { get; set; }
        public CategoryService _categoryService { get; set; }

        public CategoryController(SmartAdmin.WebUI.Services.Service service, CategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _categoryService.GetCategories();
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
                CategoryCreateViewModel model = new CategoryCreateViewModel()
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
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var CategoryId = await _categoryService.Create(model);
                    if (CategoryId > 0)
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
        public async Task<IActionResult> GetCategoriesByServiceId(int ServiceId)
        {
            try
            {
                var model = await _categoryService.GetCategoriesByServiceId(ServiceId);
                return PartialView("_CategoriesOptions", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int CategoryId)
        {
            try
            {
                var model = await _categoryService.GetCategoryById(CategoryId);
                if (model is not null)
                {
                    model.Services = await _service.GetServicesWithoutParents();
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
        public async Task<IActionResult> Edit(CategoryCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid && model.CategoryId.HasValue)
                {
                    var StartPageId = await _categoryService.Edit(model);
                    if (StartPageId is not null)
                    {
                        // return RedirectToAction("View", StartPageId);
                        return RedirectToAction("Index");
                    }
                    return View("Error404");
                }
                model.Services = await _service.GetServicesWithoutParents();
                return View("Create", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int CategoryId)
        {
            try
            {
                var status = await _categoryService.Delete(CategoryId);
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
    }
}

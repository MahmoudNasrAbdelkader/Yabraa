using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;

namespace SmartAdmin.WebUI.Controllers
{
    [Authorize(Permissions.Services.Manage)]
    public class ServicesController : Controller
    {
        public SmartAdmin.WebUI.Services.Service _service { get; set; }
        public ServicesController(SmartAdmin.WebUI.Services.Service service )
        {
            _service = service;
        }
        public  async Task<IActionResult> Index()
        {
            try
            {
               var model = await  _service.GetServices();
                return  View(model);
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
                //ViewBag.parentsServices = await _service.GetAvailableParentService();

                ViewBag.serviceTypes = await _service.GetServiceTypes();

                return View(new ServicesCreateViewModel());
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpPost]
        public async Task<IActionResult> Create(ServicesCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                  var serviceId = await _service.Create(model);
                    if (serviceId > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                //ViewBag.parentsServices = await _service.GetAvailableParentService();
                ViewBag.serviceTypes = await _service.GetServiceTypes();
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int ServiceId)
        {
            try
            {
                var model = await _service.GetServiceById(ServiceId);
                if (model is not null)
                {
                    //ViewBag.parentsServices = await _service.GetAvailableParentService(model.ParentServiceId);
                    ViewBag.serviceTypes = await _service.GetServiceTypes(model.ServiceTypeId);
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
        public async Task<IActionResult> Edit(ServicesCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid && model.ServiceId.HasValue)
                {
                    var ServiceId = await _service.Edit(model);
                    if (ServiceId is not null)
                    {
                        // return RedirectToAction("View", StartPageId);
                        return RedirectToAction("Index");
                    }
                    return View("Error404");
                }
                //ViewBag.parentsServices = await _service.GetAvailableParentService(model.ParentServiceId);
                ViewBag.serviceTypes = await _service.GetServiceTypes(model.ServiceTypeId);
                return View("Create", model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int ServiceId)
        {
            try
            {
                var status = await _service.Delete(ServiceId);
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

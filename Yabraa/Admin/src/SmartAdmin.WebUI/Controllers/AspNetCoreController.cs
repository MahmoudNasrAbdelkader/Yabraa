using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAdmin.WebUI.ViewModel;

namespace SmartAdmin.WebUI.Controllers
{
    [Authorize]   
    public class AspNetCoreController : Controller
    {
        public IActionResult Welcome() => View();
        public IActionResult Interactive() => View();
        public IActionResult Editions() => View();
        public IActionResult Faq() => View();
    }
}

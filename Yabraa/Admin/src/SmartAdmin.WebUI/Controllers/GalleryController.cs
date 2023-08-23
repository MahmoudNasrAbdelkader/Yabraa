using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YabraaEF;

namespace SmartAdmin.WebUI.Controllers
{
    public class GalleryController : Controller
    {
        public ApplicationDbContext _context { get; set; }
        private readonly IWebHostEnvironment _env;
        public GalleryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _context.Gallery.Select(c=>c.Path).ToListAsync();
                return View(model);
            }
            catch (System.Exception ex)
            {

                return View("Error404", ex);
            }

        }
    }
}

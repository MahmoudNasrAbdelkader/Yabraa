using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Yabraa.Helpers;
using Yabraa.Services;
using YabraaEF.Models;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
       

        private readonly GalleryService _service;
       
        public GalleryController(GalleryService service)
        {
            _service = service;
           
        }
        [HttpGet]
        public async Task<IActionResult> get()
        {
            try
            {
               var GalleryImages = await _service.GetGalleryImages();
                if (GalleryImages is not null)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { GalleryImages = GalleryImages } });

                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
    }
}

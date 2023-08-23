using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Yabraa.Helpers;
using Yabraa.Services;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartPagesController : ControllerBase
    {
        private readonly StartPagesService _service;

        public StartPagesController(StartPagesService service)
        {
            _service = service;

        }

        [HttpGet]
        public async Task<IActionResult> get()
        {
            try
            {
                var Pages = await _service.GetStartPages();
                if (Pages is not null && Pages.Count > 0)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = Pages});

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

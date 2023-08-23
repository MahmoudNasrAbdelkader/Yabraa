using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Yabraa.Const;
using Yabraa.DTOs;
using Yabraa.Helpers;
using Yabraa.Services;
using YabraaEF.Const;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly MainService _mainService;
        public ServicesController(MainService mainService)
        {
            _mainService = mainService; 
        }
        [HttpGet("GetServiceTypes")]
        public IActionResult GetServiceTypes()
        {
            try
            {
                var result = _mainService.GetServiceTypes();
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }

        [HttpGet("servicesDetails")]
        public IActionResult GetServicesDetails(int serviceTypeId)
        {
            try
            {
              var result =   _mainService.GetServicesDetails(serviceTypeId);               
               return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        [HttpGet("getDates")]
        public IActionResult GetDates()
        {
            try
            {
                DateTime dateTime = General.GetKSATimeZoneNow();
                List<DateDto> GetDates = new List<DateDto>();
                DateDto dateDto;
                for (int i = 0; i < 30; i++)
                {
                    dateDto = new DateDto()
                    {
                        Year = dateTime.Year,
                        MonthName = dateTime.ToString("MMMM"),
                        MonthNumber = dateTime.ToString("MM"),
                        DayName  = dateTime.ToString("dddd"),
                        DayOfMonth = dateTime.ToString("dd"),
                        MonthShortName = dateTime.ToString("MMM")                        
                    };

                    dateTime = dateTime.AddDays(1);
                    GetDates.Add(dateDto);
                }

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = GetDates });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        //[AllowAnonymous]
        //[HttpGet("setData")]
        //public IActionResult setData()
        //{
        //    try
        //    {
        //        _mainService.InsertData();
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = ex.Message, OperationMessage = ex.InnerException?.Message*/ });
        //    }
        //}

    }
}

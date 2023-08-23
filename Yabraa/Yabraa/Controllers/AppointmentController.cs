using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Security.Claims;
using Yabraa.DTOs;
using Yabraa.Helpers;
using Yabraa.Services;
using YabraaEF;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentsService _appointmentsService;
        private readonly ApplicationDbContext _dbContext;
        public AppointmentController(AppointmentsService appointmentsService, ApplicationDbContext dbContext)
        {
            _appointmentsService = appointmentsService;
            _dbContext = dbContext;
        }
        [Authorize]
        [HttpGet("GetAppointmentsByUser")]
        public async Task<IActionResult> GetAppointmentsByUser()
        {
            try
            {
                string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                var model = await _appointmentsService.GetAppointmentsByUserId(userId);
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = model });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
        [Authorize]
        [HttpGet("GetAppointmentDetailsByAppointmentId")]
        public async Task<IActionResult> GetAppointmentDetailsByAppointmentId(long AppointmentId)
        {
            try
            {
                string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                var model = await _appointmentsService.GetAppointmentdByUserIdAndVisitDetailsId(userId, AppointmentId);
                if (model is not null)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = model });
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });


            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
        [Authorize]
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit(AppointmentEditDto appointmentEditDto)
        {
            try
            {
                int ErrorNumber = _appointmentsService.AppointmentEditValidatione(appointmentEditDto);
                if (ErrorNumber == -1)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

                }
                else if (ErrorNumber == -2)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Sorry,this appointment is not pending", ErrorMessageAr = "آسف ، هذا الموعد غير معلق" });

                }
                else if (ErrorNumber == -3)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Sorry,The appointment date has already passed", ErrorMessageAr = "آسف ، لقد مر تاريخ الموعد بالفعل" });

                }
                if (appointmentEditDto.LocationLongitude < 1)
                {
                    ModelState.AddModelError("LocationLongitude", "LocationLongitude is required");
                }
                if (appointmentEditDto.LocationLatitude < 1)
                {
                    ModelState.AddModelError("LocationLatitude", "LocationLatitude is required");
                }
                string errorMessagesEn = "";
                string errorMessagesAr = "";
                if (!ModelState.IsValid)
                {
                    errorMessagesEn += $"Please check your input. ";
                    errorMessagesAr += $"يرجى التحقق من المدخلات الخاصة بك. ";
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        errorMessagesEn += $"{error.ErrorMessage}. ";
                        errorMessagesAr += $"{error.ErrorMessage}. ";
                    }

                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn });
                }

                string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                var model = await _appointmentsService.EditAppointment(userId, appointmentEditDto);
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = model });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
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
    public class UserFamilyController : ControllerBase
    {
        private readonly UserFamilyService _userFamilyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        public UserFamilyController(UserFamilyService userFamilyService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _userFamilyService = userFamilyService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserFamily()
        {
            try
            {
                string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
                var result = _userFamilyService.GetUserFamily(userId);              
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddUserFamily(UserFamilyDto userFamilyDto)
        {
            try
            {
                string errorMessagesEn = "";
                string errorMessagesAr = "";
                if (!ModelState.IsValid)
                {
                    errorMessagesEn += $"please check your input. ";
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
                var result = _userFamilyService.AddUserFamily(userId, userFamilyDto);
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });
            }
            catch (Exception )
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        [Authorize]
        [HttpDelete("{userFamilyId}")]
        public async Task<IActionResult> DeleteUserFamily(long userFamilyId)
        {
            try
            {               
                var result = _userFamilyService.DeleteUserFamily(userFamilyId);
                if (result)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = "Deleted" });
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditUserFamily(UserFamilyDto userFamilyDto)
        {
            try
            {
                string errorMessagesEn = "";
                string errorMessagesAr = "";
                if (!ModelState.IsValid)
                {
                    errorMessagesEn += $"please check your input. ";
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
                var result = _userFamilyService.EditUserFamily(userFamilyDto , userId);
                if (result is not null)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }

        //[Authorize]
        //[HttpGet("GetAppointmentsByUser")]
        //public async Task<IActionResult> GetAppointmentsByUser()
        //{
        //    try
        //    {
        //        string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var userId = _dbContext.Users.FirstOrDefault(c => c.UserName == userName).Id;
        //        var model = await _userFamilyService.GetAppointmentsByUserId(userId);               
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = model });

        //    }
        //    catch (Exception)
        //    {
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
        //    }


        //}
    }
}

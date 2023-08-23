using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text;
using Yabraa.DTOs;
using Yabraa.Helpers;
using Yabraa.Services;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentservice;
        private ApplicationDbContext _context;
        public PaymentController(PaymentService service, ApplicationDbContext context)
        {
            _paymentservice = service;
            _context = context;
        }
        [HttpPost("GetCheckoutId")]
        public async Task<IActionResult> GetCheckoutId(PaymentDto paymentDto)
        {
            try
            {
                string errorMessage = "";
                if ( paymentDto.ServiceTypeId == 1)
                {
                    if (paymentDto.locationLongitude < 1 )
                    {
                        ModelState.AddModelError("locationLongitude", "locationLongitude is required");
                    }
                   
                }
                if ( paymentDto.ServiceTypeId == 1)
                {
                    if (paymentDto.locationLatitude < 1)
                    {
                        ModelState.AddModelError("locationLatitude", "locationLatitude is required");

                    }
                }

                if (ModelState.IsValid && paymentDto.packages is not null && paymentDto.packages.Count > 0 )
                {                  
                    bool errorFlag = true;
                  
                    foreach (var package in paymentDto.packages)
                    {
                       
                        var validationResults = new List<ValidationResult>();
                        var isValid = Validator.TryValidateObject(package, new ValidationContext(package), validationResults, true);
                        if (!isValid)
                        {
                            foreach (var validationResult in validationResults)
                            {
                                foreach (var memberName in validationResult.MemberNames)
                                {
                                    errorFlag = false;
                                    errorMessage = $" {memberName} {validationResult.ErrorMessage},";
                                }

                            }
                        }
                    }
                    if (!errorFlag)
                    {
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Please check your input ({errorMessage}).", ErrorMessageAr = $"({errorMessage}) يرجى التحقق من المدخلات الخاصة بك." });

                    }
                   
                    List<int> packageIds = paymentDto.packages.Select(c => c.packageId).ToList();
                    var totalPriceDB = _context.Packages.Where(c => packageIds.Contains(c.PackageId)).Sum(c => c.Price);
                    if (totalPriceDB != paymentDto.amount)
                    {
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Please restart the application).", ErrorMessageAr = $"الرجاء إعادة تشغيل التطبيق." });

                    }
                    string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userId = _context.Users.FirstOrDefault(c => c.UserName == userName).Id;
                    var checkoutId = await _paymentservice.Payment(paymentDto, userId, paymentDto.amount, paymentDto.currency, "DB");
                    if (!string.IsNullOrWhiteSpace(checkoutId) )
                    {
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { checkoutId = checkoutId, model = paymentDto } }) ;
                    }
                }
                foreach (var entry in ModelState.Values)
                {
                    foreach (var error in entry.Errors)
                    {
                        errorMessage = $" {error.ErrorMessage},";

                    }
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Please check your input ({errorMessage}).", ErrorMessageAr = $" ({errorMessage}) . يرجى التحقق من المدخلات الخاصة بك." });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
      
        [HttpGet("GetCheckoutStatus")]
        public async Task<IActionResult> GetCheckoutStatus(string CheckoutId, int PaymentMethodId)
        {
            try
            {
               
                var responseData =  _paymentservice.GetCheckoutStatus(CheckoutId, PaymentMethodId);
                if (responseData is not null && responseData.Count > 0)
                {
                    string code = responseData["result"]["code"];
                    string description = responseData["result"]["description"];
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { code, description } });

                }

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Please check your input.", ErrorMessageAr = $". يرجى التحقق من المدخلات الخاصة بك." });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
        [HttpGet("GetTransactionInfo")]
        public async Task<IActionResult> GetTransactionInfo(string CheckoutId)
        {
            try
            {

                var responseData = _paymentservice.GetTransactionInfo(CheckoutId);
                //if (responseData is not null && responseData.Count > 0)
                //{
                //    string code = responseData["result"]["code"];
                //    string description = responseData["result"]["description"];
                  

                //}
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { responseData } });
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"Please check your input.", ErrorMessageAr = $". يرجى التحقق من المدخلات الخاصة بك." });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }

        [HttpGet("HistoryPayment")]
        public async Task<IActionResult> HistoryPayment()
        {
            try
            {
                string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = _context.Users.FirstOrDefault(c => c.UserName == userName).Id;
                var responseData = _paymentservice.GetHistoryPayment(userId);             

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new {  responseData.Result } });

            }
            catch (Exception ex )
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }
        [HttpGet("GetPaymentMethods")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {                
                var responseData = _paymentservice.GetPaymentMethods();
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { responseData.Result } });

            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }


        }

    }
}

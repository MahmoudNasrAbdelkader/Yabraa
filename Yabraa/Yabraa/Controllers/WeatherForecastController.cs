using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using Yabraa.IServices;

namespace Yabraa.Controllers
{
    [Authorize(Roles ="User")]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
         };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IEmailSender _emailSender;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var rng = new Random();
            var message = new Message(new string[] { "mahmoudnasrabdelkader@gmail.com" }, " مرحبا بك - يبرأ ", $"<div style='direction: rtl; '> <strong>عزيزي / عزيزتي : مستخدم </strong><p>شكرا لانضمامك معنا في يبرأ</p>   <br/> <a href='tel: +2010199764875777812'>+2010197657977812</a> <br/> <a href = 'https://api.whatsapp.com/send?phone=+20101997781276'> WhatsApp </a> <br/> <a href = 'mailto:headvccvxoffice@opera-city.com' >Email</a> <br/> </div> ",null);
            await _emailSender.SendEmailAsync(message, true);
            return Ok("Done.");
        }
        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Post()
        {          
            var files = Request.Form.Files.Any() ? Request.Form.Files : new FormFileCollection();
            var message = new Message(new string[] { "mahmoudnasrabdelkader@gmail.com" }, "مرحبا بك - يبرأ", $"<h2 style='color:red;'>This is the content from our mail with attachments</h2>", files);
            await _emailSender.SendEmailAsync(message);
            return Ok("Done.");
        }
    }
}


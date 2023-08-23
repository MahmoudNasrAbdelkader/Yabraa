using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using YabraaEF.Models;
using System.Linq;
using SmartAdmin.WebUI.ViewModel;
using Microsoft.EntityFrameworkCore;
using YabraaEF;
using static SmartAdmin.WebUI.ViewModel.Permissions;

namespace SmartAdmin.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly string smtpHost, smtpUsername, smtpPassword, smtpFrom, toollink;
        private readonly int smtpPort;
        private readonly bool smtpEnableSSL;
        public ApplicationDbContext _context { get; set; }

        public AccountController(IConfiguration configuration, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            toollink = configuration.GetSection("Core").GetSection("Tool_Link").Value;
            smtpHost = configuration.GetSection("Core").GetSection("SMTP_Host").Value;
            smtpUsername = configuration.GetSection("Core").GetSection("SMTP_Username").Value;
            smtpPassword = configuration.GetSection("Core").GetSection("SMTP_Password").Value;
            smtpFrom = configuration.GetSection("Core").GetSection("SMTP_From").Value;
            smtpPort = int.Parse(configuration.GetSection("Core").GetSection("SMTP_Port").Value);
            smtpEnableSSL = bool.Parse(configuration.GetSection("Core").GetSection("SMTP_EnableSSL").Value);
            _context=context;
        }

        //[HttpGet]
        //public async Task<string> GetCurrentUserId()
        //{
        //    ApplicationUser usr = await GetCurrentUserAsync();
        //    return usr?.Id;
        //}
        //private Task<ApplicationUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);
        public IActionResult Index()
        {


            return View();
        }

        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }


        public void SendMail(string to, string firstname, string seconedname, string username, string password)
        {
            try
            {
                string mailbody = $"Dear {firstname} {seconedname},<br/><br/> Your WPM new username is: {username} <br/> Your Default Password is: {password} <br/> You will be requested to change this password after your first login <bt/>Please click the following link to access the tool <br/><a href='{toollink}'>Click here</a>";

                if (string.IsNullOrWhiteSpace(to)) return;
                SmtpClient client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = smtpEnableSSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = string.IsNullOrWhiteSpace(smtpUsername);
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                MailMessage mailMessage = new MailMessage();
                mailMessage.IsBodyHtml = true;
                mailMessage.From = new MailAddress(smtpFrom);
                mailMessage.To.Add(to);
                mailMessage.Subject = $"WPM – A new user is created for you";
                mailMessage.Body = mailbody;
                client.SendAsync(mailMessage, null);
            }
            catch
            {
                return;
            }
        }

        public void sendemail(string emailto, string username, string password)
        {
            string to = emailto; //To address    
            string from = "safymohamed925@gmail.com"; //From address    
            MailMessage message = new MailMessage(from, to);

            // string mailbody = $"  عزيزي مستخدم موقع الكشاف العربي يمكنك الان اعادة تعيين كلمة السر من خلال الضغط علي هذا الرابط  <a href=\"{link}\"> اضغط هنا  </a> ";
            string mailbody = $"An account has been created for you on the (WPM) System, and you can now login with the following data <br/> username: {username} <br/> password: {password}";

            message.Subject = "WPM – A new user is created for you";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("safymohamed925@gmail.com", "passwordhere"); ;
            try
            {
                client.Send(message);
            }
            catch
            {

            }

        }


        [Authorize(Permissions.Users.Create)]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            
            int usersCount = userManager.Users.Count();
            if (usersCount < 300)
            {
            
                var Countries = await _context.Countries.ToListAsync();
        
                ViewData["roles"] = new SelectList(roleManager.Roles.Where(a => a.Id != "4ee900da-b09e-49f0-8a08-2ebd111058c8").OrderBy(a => a.Name), "Name", "Name");
                //   var roles = roleManager.Roles;
                ViewData["countries"] = new SelectList(Countries, "CountryCode", "CountryArNationality", "SA");

                return View();
            }
            else
            {
                return View("MaxUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    BirthDate = model.BirthDate,
                    CountryCode = model.CountryCode,
                    Gender = model.Gender,
                    IsSystemUser =true,
                    CrateDateTime =DateTime.Now,
                    IdOrPassport =model.IdOrPassport,
                    PhoneNumber = model.PhoneNumber                    
                };

                // Store user data in AspNetUsers database table
                string password = GenerateRandomPassword();
                var result = await userManager.CreateAsync(user, password);
                IdentityResult result2 = null;
                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    result2 = await userManager.AddToRoleAsync(user, model.RoleName);
                    //   await signInManager.SignInAsync(user, isPersistent: false);
                    // return RedirectToAction("index", "home");

                    if (result2.Succeeded)
                    {
                        SendMail(user.Email, user.FirstName, user.LastName, user.UserName, password);
                        return RedirectToAction("ListUsers", "Administration");
                    }
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["roles"] = new SelectList(roleManager.Roles.Where(a => a.Id != "4ee900da-b09e-49f0-8a08-2ebd111058c8").OrderBy(a => a.Name), "Name", "Name");
            var Countries = await _context.Countries.ToListAsync();
            ViewData["countries"] = new SelectList(Countries, "CountryCode", "CountryArNationality", model.CountryCode);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]

        

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    ApplicationUser user = await signInManager.UserManager.FindByNameAsync(model.UserName);


                    if (user.EmailConfirmed == false)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("EditUserByuser", "Administration");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }

            return View(model);
        }
        //[HttpGet]
        //public async Task<IActionResult> ChangeTheme(int ThemeId)
        //{
        //    try
        //    {
        //        var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //        if (user is not null)
        //        {
        //            user.ThemeId = ThemeId;
        //            var result = await userManager.UpdateAsync(user);
        //            if (result.Succeeded)
        //            {
        //                return RedirectToAction("index", "home");
        //            }
        //        }
        //        return View("Error");
        //    }
        //    catch (Exception)
        //    {
        //        return View("Error");
        //    }
        //}
    }
}

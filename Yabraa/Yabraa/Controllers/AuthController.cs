using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Twilio;
using Twilio.Http;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;
using Yabraa.Const;
using Yabraa.DTOs;
using Yabraa.Helpers;
using Yabraa.IServices;
using Yabraa.Services;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISMSService _smsSender;
        private readonly IConfiguration _configuration;
        private readonly JWT _jwt;
        private ApplicationDbContext _context;


        public AuthController
            (   AuthService authService,
                UserManager<ApplicationUser> userManager,
                IEmailSender emailSender,
                SignInManager<ApplicationUser> signInManager,
                ISMSService smsSender,
                IConfiguration configuration,
                 IOptions<JWT> jwt,
                 ApplicationDbContext context
            )
        {
            _authService = authService;
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _smsSender = smsSender;
            _configuration = configuration;
            _jwt = jwt.Value;
            _context = context;
        }
        [AllowAnonymous]
        [HttpGet("PrepareRegistration")]
        public  IActionResult PrepareRegistration()
        {
            try
            {                
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { Nationalities = _authService.GetCountries().Result } });
            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later" , ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }
        }
        [AllowAnonymous]
        [HttpPost("userInputsValidation")]
        public async Task<IActionResult> UserInputsValidation([FromBody] RegisterModel model)
        {
            try
            {
                string errorMessagesEn = "";
                string errorMessagesAr = "";
              
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    errorMessagesEn += "please enter password";
                    errorMessagesAr += "الرجاء إدخال كلمة المرور ";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }
                else
                {
                    model.Password = model.Password.Trim();
                    if (model.Password.Length < 6)
                    {

                        errorMessagesEn += "Password must be at least 6 characters.";
                        errorMessagesAr += "كلمة المرور من 6 أحرف على الأقل";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }
                }
                model.PhoneNumber = model.PhoneNumber.Trim(); 

                if (!(!string.IsNullOrEmpty(model.PhoneNumber) && !string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber.Length == 9))
                {
                    errorMessagesEn += "The mobile number must consist of 9 digits.";
                    errorMessagesAr += "يجب أن يكون رقم الجوال مكون من 9 أرقام .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }
                DateTime date;

                if (!DateTime.TryParseExact(model.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    errorMessagesEn += "The date is incorrect.";
                    errorMessagesAr += "التاريخ غير صحيح.";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }


                if (date > DateTime.Now)
                {
                    errorMessagesEn += "Date of birth cannot be later than the current date. ";
                    errorMessagesAr += "لا يجب أن يكون تاريخ الميلاد أحدث من التاريخ الحالي .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }
                int minYear = 1900;
                if (date.Year < minYear)
                {
                    errorMessagesEn += "Date of birth must be after 1900. ";
                    errorMessagesAr += "1900 تاريخ الميلاد يجب أن يكون بعد عام .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    bool isMatch = Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$");
                    if (!IsValidEmail(model.Email))
                    {
                        errorMessagesEn += "The email address you entered is invalid. ";
                        errorMessagesAr += "البريد الإلكتروني الذي أدخلته غير صالح.";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                    }

                }
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

                //var result = await _authService.RegisterAsync(model);

                if (await _authService.ChechIfPhoneNumberAlreadyRegistered(model.PhoneNumber))
                {
                    errorMessagesEn += "Phone Number is already registered!";
                    errorMessagesAr += "رقم الهاتف مسجل بالفعل!";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }
                model.VerificationCode = "9524";                 

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = model });
            }
            catch (Exception)
            {

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            try
            {
                string errorMessagesEn = "";
                string errorMessagesAr = "";
              

               
                if (string.IsNullOrWhiteSpace(model.Password))
                 {
                    errorMessagesEn += "please enter password";
                    errorMessagesAr += "الرجاء إدخال كلمة المرور ";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                 }
                else
                {
                    model.Password = model.Password.Trim();
                    if (model.Password.Length < 6)
                    {

                        errorMessagesEn += "Password must be at least 6 characters.";
                        errorMessagesAr += "كلمة المرور من 6 أحرف على الأقل";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }
                }
                model.PhoneNumber = model.PhoneNumber.Trim();

                if (!(!string.IsNullOrEmpty(model.PhoneNumber) && !string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber.Length == 9))
                {
                    errorMessagesEn += "The mobile number must consist of 9 digits.";
                    errorMessagesAr += "يجب أن يكون رقم الجوال مكون من 9 أرقام .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }
                    
                DateTime date;

                if (!DateTime.TryParseExact(model.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    errorMessagesEn += "The date is incorrect.";
                    errorMessagesAr += "التاريخ غير صحيح.";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }

                if (date > DateTime.Now)
                {
                    errorMessagesEn += "Date of birth cannot be later than the current date. ";
                    errorMessagesAr += "لا يجب أن يكون تاريخ الميلاد أحدث من التاريخ الحالي .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }
                int minYear = 1900;
                if (date.Year < minYear)
                {
                    errorMessagesEn += "Date of birth must be after 1900. ";
                    errorMessagesAr += "1900 تاريخ الميلاد يجب أن يكون بعد عام .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }               
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    bool isMatch = Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$");
                    if (!IsValidEmail(model.Email))
                    {
                        errorMessagesEn += "The email address you entered is invalid. ";
                        errorMessagesAr += "البريد الإلكتروني الذي أدخلته غير صالح.";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                    }
                    
                }

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
                var result = await _authService.RegisterAsync(model);

                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    if (result.Message.Contains("Phone Number is already registered!"))
                    {
                        errorMessagesEn += result.Message;
                        errorMessagesAr += "رقم الهاتف مسجل بالفعل!";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }else if (result.Message.Contains("The date is incorrect!"))
                    {
                        errorMessagesEn += "The date is incorrect.";
                        errorMessagesAr += "التاريخ غير صحيح.";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }
                    else
                    {
                        errorMessagesEn += $"An error has occurred, {result.Message}"; 
                        errorMessagesAr += $"لقد حدث خطأ, {result.Message}";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                        
                    }
                    
                }
               
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });
            }
            catch (Exception)
            {

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

            }

        }
        bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("phoneVerificationCode")]
        public async Task<IActionResult> PhoneVerificationCode([FromBody] PhoneVerificationCodeDto model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.PhoneNumber);
                if (user.verificationCode.HasValue && user.verificationCode.Value == model.Code)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    var result = await _authService.GetTokenAsync(new TokenRequestModel { Password = model.Password, PhoneNumber = model.PhoneNumber });

                    if (!string.IsNullOrWhiteSpace(result.Message))
                    {
                        string errorMessagesEn = "";
                        string errorMessagesAr = "";
                        errorMessagesEn += $"An error has occurred, {result.Message}";
                        errorMessagesAr += $"لقد حدث خطأ, {result.Message}";

                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                    }

                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });
                }
                else
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"The verification code is incorrect.", ErrorMessageAr = "رمز التحقق غير صحيح ." });
                }
            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

            }
            
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
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
                // var user = await _userManager.FindByNameAsync(model.PhoneNumber);
                // if (user.TwoFactorEnabled && !user.PhoneNumberConfirmed)
                // {

                //    await _signInManager.SignOutAsync();
                //    await _signInManager.PasswordSignInAsync(user, model.Password, false, true);
                //    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                //    var resultSms = _smsSender.Send($"+2{user.PhoneNumber}", $"OTP Confrimation : {token}");

                //    if (!string.IsNullOrEmpty(resultSms.Result.ErrorMessage))
                //        return BadRequest(resultSms.Result.ErrorMessage);

                //    return Ok("OTP confirmation sent");
                // }

                var result = await _authService.GetTokenAsync(model);

                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    if (result.Message.Contains("UserName or Password is incorrect!"))
                    {
                        errorMessagesEn += result.Message;
                        errorMessagesAr += "اسم المستخدم أو كلمة المرور غير صحيحة!";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }
                    else
                    {
                        errorMessagesEn += $"An error has occurred, {result.Message}";
                        errorMessagesAr += $"لقد حدث خطأ, {result.Message}";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                    }
                }             
               
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = result });
            }
            catch (Exception)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
            }

        }
        [HttpPost]
        [Route("login2FA")]
        public async Task<IActionResult> LoginWithOTP([FromBody] LoginOTPDTo model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            var signIn = await _signInManager.TwoFactorSignInAsync("Email", model.Code, false, false);
            if (signIn.Succeeded)
            {
                if (user != null)
                {
                    user.PhoneNumberConfirmed = true;
                    var result = await _userManager.UpdateAsync(user);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var jwtToken = await _authService.GetTokenAsync(model.PhoneNumber);

                    return Ok(jwtToken);
                }
            }
            return BadRequest($"Invalid Code");
        }
        [HttpPost("TwoFactorAuthentication")]
        public async Task<IActionResult> TwoFactorAuthentication(string TwoFactorCode, string Phone,string Password ,bool RememberMe)
        {
          
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
            var code = TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorSignInAsync(Phone, code, RememberMe,RememberMe);           

            return RedirectToAction("GetTokenAsync", new TokenRequestModel() { PhoneNumber = Phone, Password = Password });

        }
        [Authorize(Roles = "Admin")]
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            string errorMessagesEn = "";
            string errorMessagesAr = "";
            // check if user exists with email/phone
            forgotPasswordDto.PhoneNumber = forgotPasswordDto.PhoneNumber.Trim();
            if (!(!string.IsNullOrEmpty(forgotPasswordDto.PhoneNumber) && !string.IsNullOrWhiteSpace(forgotPasswordDto.PhoneNumber) && forgotPasswordDto.PhoneNumber.Length == 9))
            {
                errorMessagesEn += "The mobile number must consist of 9 digits.";
                errorMessagesAr += "يجب أن يكون رقم الجوال مكون من 9 أرقام .";
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

            }
            var user = await _userManager.FindByNameAsync(forgotPasswordDto.PhoneNumber);
            if (user == null)
            {
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = "User not found.", ErrorMessageAr = "لم يتم العثور على المستخدم." });

            }

            // generate password reset code and store in database
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // send password reset code to user's phone number
            //await _smsOrMobileService.SendAsync(forgotPasswordDto.PhoneNumber, $"Your password reset code is {code}.");
            ForgotPasswordResponseDto forgotPasswordResponseDto = new ForgotPasswordResponseDto()
            {
                PhoneNumber = forgotPasswordDto.PhoneNumber,
                Token = code,
                VerificationCode = "9524"
            };
            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data= forgotPasswordResponseDto });

        }
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
         
            try
            {
                // find user by email/phone
                var user = await _userManager.FindByNameAsync(resetPasswordDto.PhoneNumber);
                if (user == null)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = "User not found.", ErrorMessageAr = "لم يتم العثور على المستخدم." });
                }
                resetPasswordDto.Password = resetPasswordDto.Password.Trim();
                resetPasswordDto.Password = resetPasswordDto.Password.Trim();
                if (resetPasswordDto.Password.Length < 6)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = "Password must be at least 6 characters.", ErrorMessageAr = "كلمة المرور من 6 أحرف على الأقل" });
                }
                if (!string.Equals(resetPasswordDto.Password, resetPasswordDto.ConfirmPassword))
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = "The password does not match.", ErrorMessageAr = "كلمة السر غير متطابقة" });
                }

                // reset user's password using the password reset code
                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, (resetPasswordDto.Password + "v%D9"));
                if (!result.Succeeded)
                {
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new {PhoneNumber = resetPasswordDto.PhoneNumber,  MessageAr = "إعادة تعيين كلمة المرور بنجاح.", MessageEn = "Password reset successfully." } });

            }
            catch (Exception)
            {

                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });

            }

           
        }
        [HttpPost("guestlogin")]
        public IActionResult GuestLogin()
        {
            // generate a new guest access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, "Guest")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = new { Token = tokenString } });

        }
        [Authorize(Roles = "User")]
        [HttpPut("EditeAccount")]
        public async Task<IActionResult> EditeAccount(EditUserDto userDto)
        {
            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = _context.Users.FirstOrDefault(c => c.UserName == userName).Id;

            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
            {
                string errorMessagesEn = "";
                string errorMessagesAr = "";
                if (!string.IsNullOrWhiteSpace(userDto.Password))
                {
                    userDto.Password = userDto.Password.Trim();
                    if (userDto.Password.Length < 6)
                    {

                        errorMessagesEn += "Password must be at least 6 characters.";
                        errorMessagesAr += "كلمة المرور من 6 أحرف على الأقل";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                    }
                }

                DateTime date;

                if (!DateTime.TryParseExact(userDto.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    errorMessagesEn += "The date is incorrect.";
                    errorMessagesAr += "التاريخ غير صحيح.";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }

                if (date > DateTime.Now)
                {
                    errorMessagesEn += "Date of birth cannot be later than the current date. ";
                    errorMessagesAr += "لا يجب أن يكون تاريخ الميلاد أحدث من التاريخ الحالي .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });
                }
                int minYear = 1900;
                if (date.Year < minYear)
                {
                    errorMessagesEn += "Date of birth must be after 1900. ";
                    errorMessagesAr += "1900 تاريخ الميلاد يجب أن يكون بعد عام .";
                    return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                }
                if (!string.IsNullOrWhiteSpace(userDto.Email))
                {
                    bool isMatch = Regex.IsMatch(userDto.Email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$");
                    if (!IsValidEmail(userDto.Email))
                    {
                        errorMessagesEn += "The email address you entered is invalid. ";
                        errorMessagesAr += "البريد الإلكتروني الذي أدخلته غير صالح.";
                        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = errorMessagesEn, ErrorMessageAr = errorMessagesAr });

                    }

                }
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
               // _authService.EditeAccount( userDto , userId);
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.BirthDate = date;
                user.Gender = (userDto.Gender.ToLower() == "male" ? (int)Gender.Male : (int)Gender.Female);
                user.CountryCode = userDto.CountryCode;
                user.IdOrPassport = userDto.IdOrPassport;
               
                _context.SaveChanges();
                var userFamily = _context.UserFamilies.FirstOrDefault(c => c.ApplicationUserId == userId && c.IsOwner);
                if (userFamily is not null)
                {
                    userFamily.BirthDate = date;
                    userFamily.Name = $"{user.FirstName} {user.LastName}";
                    userFamily.Gender = user.Gender;
                    _context.SaveChanges();
                }
                if (!string.IsNullOrWhiteSpace(userDto.Password))
                {
                    //var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, (userDto.Password + "v%D9"));
                    //user.PasswordHash = newPasswordHash;
                    //var result = await _userManager.UpdateAsync(user)
                     var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, code, (userDto.Password + "v%D9"));
                        if (!result.Succeeded)
                        {
                            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
                        }
                }
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = "Done" });

            }

            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
        }

        [Authorize(Roles = "User")]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = _context.Users.FirstOrDefault(c => c.UserName == userName).Id;

            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
            {
                 user.Deleted = true;
                _context.SaveChanges();
                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = "Deleted" });

            }

            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
        }
        [Authorize(Roles = "User")]
        [HttpGet("GetUserInformation")]
        public async Task<IActionResult> GetUserInformation()
        {
            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = _context.Users.Include(c=>c.Country).FirstOrDefault(c => c.UserName == userName).Id;

            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
            {
                
                UserInformationDto userInformationDto = new UserInformationDto()
                {
                    BirthDate = user.BirthDate.ToString("yyyy-MM-dd"),
                    CountryCode= user.CountryCode,
                    CountryNameAr =user.Country.CountryArName,
                    CountryNameEn = user.Country.CountryEnName,
                    CrateDateTime = user.CrateDateTime.ToString("yyyy-MM-dd HH:mm tt"),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    NationalityAr = user.Country.CountryArNationality,
                    NationalityEn = user.Country.CountryEnNationality,
                    PhoneNumber = user.PhoneNumber,
                    IdOrPassport = user.IdOrPassport,
                    LastName = user.LastName,
                    UserName = userName,
                    Gender = user.Gender == 0 ? "Male" : "Female"
                };


                return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK, Data = userInformationDto });

            }

            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest, ErrorMessageEn = $"An error has occurred, please try again later", ErrorMessageAr = "لقد حدث خطأ، يرجى المحاولة في وقت لاحق" });
        }
        //[HttpPost("forgotPassword")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = ModelState */});


        //        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        //        if (user == null)
        //            //return BadRequest("Invalid Request");
        //            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = "Invalid Request" */});

        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var param = new Dictionary<string, string?>
        //        {
        //            {"token", token },
        //            {"email", forgotPasswordDto.Email }
        //        };
        //        //var callback = QueryHelpers.AddQueryString(forgotPasswordDto.ClientURI, param);\
        //        var callback = QueryHelpers.AddQueryString(String.IsNullOrEmpty(forgotPasswordDto.ClientURI) ? string.Empty : forgotPasswordDto.ClientURI, param);
        //        var message = new Message(new string[] { user.Email }, "Reset password", $" you can Reset password fom the link :  {callback}", null);
        //        await _emailSender.SendEmailAsync(message);

        //        //  return Ok();
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK/*, OperationMessage = "Done"*/ });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = ex.Message, OperationMessage = ex.InnerException?.Message*/ });
        //    }

        //}
        //[HttpPost("resetPassword")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            //return BadRequest();
        //            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = ModelState*/ });
        //        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        //        if (user == null)
        //            //return BadRequest("Invalid Request");
        //            return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = "Invalid Request"*/ });

        //        var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
        //        if (!resetPassResult.Succeeded)
        //        {
        //            var errors = resetPassResult.Errors.Select(e => e.Description);
        //            return BadRequest(new { Errors = errors });
        //        }
        //        // return Ok();
        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.OK/*, OperationMessage = "Done"*/ });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Ok(new ResponseVM() { StatusCode = HttpStatusCode.BadRequest/*, Error = ex.Message, OperationMessage = ex.InnerException?.Message */});

        //    }

        //}
        //[HttpGet("refreshToken")]
        //public async Task<IActionResult> RefreshToken()
        //{
        //    var refreshToken = Request.Cookies["refreshToken"];

        //    var result = await _authService.RefreshTokenAsync(refreshToken);

        //    if (!result.IsAuthenticated)
        //        return BadRequest(result);

        //    SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        //    return Ok(result);
        //}

        //[HttpPost("revokeToken")]
        //public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        //{
        //    var token = model.Token ?? Request.Cookies["refreshToken"];

        //    if (string.IsNullOrEmpty(token))
        //        return BadRequest("Token is required!");

        //    var result = await _authService.RevokeTokenAsync(token);

        //    if (!result)
        //        return BadRequest("Token is invalid!");

        //    return Ok();
        //}

        //private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Expires = expires.ToLocalTime()
        //    };

        //    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        //}
        //[HttpPost("send")]
        //public IActionResult Send(SendSMSDto dto)
        //{
        //    var result = _smsService.Send(dto.MobileNumber, dto.Body);

        //    if (!string.IsNullOrEmpty(result.ErrorMessage))
        //        return BadRequest(result.ErrorMessage);

        //    return Ok(result);
        //}
    }
}

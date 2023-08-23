using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Yabraa.Const;
using Yabraa.DTOs;
using Yabraa.Helpers;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private ApplicationDbContext _dbContext;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _dbContext = dbContext;
        }
        public async Task<List<NationalityDTO>> GetCountries()
        {
            return  await _dbContext.Countries.Select(c=> new NationalityDTO { CountryCode = c.CountryCode , CountryArNationality =c.CountryArNationality , CountryEnNationality =c.CountryEnNationality }).ToListAsync();
        }
        public async Task<bool> ChechIfPhoneNumberAlreadyRegistered(string PhoneNumber)
        {
            if (await _userManager.FindByNameAsync(PhoneNumber) is not null)
                return true;
            return false;
        }
        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            //if (await _userManager.FindByEmailAsync(model.Email) is not null)
            //    return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.PhoneNumber) is not null)
                return new AuthModel { Message = "Phone Number is already registered!" };
            DateTime date;

            if (!DateTime.TryParseExact(model.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return new AuthModel { Message = "The date is incorrect!" };              
            }
            string gander = model.Gender.ToLower().Trim();
            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber.Trim(),
                Email = model.Email?.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                //TwoFactorEnabled =true,
                BirthDate = date,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Gender = (gander.ToLower() == "male" ? (int)Gender.Male : (int)Gender.Female),
                CountryCode = model.CountryCode.Trim(),
                IdOrPassport=model.IdOrIqamaOrPassport?.Trim(),  
                PhoneNumberConfirmed = true,
                CrateDateTime=DateTime.UtcNow,
                Deleted = false
            };

            var result = await _userManager.CreateAsync(user, (model.Password + "v%D9"))   ;

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            //var refreshToken = GenerateRefreshToken();
            //user.RefreshTokens?.Add(refreshToken);
            //await _userManager.UpdateAsync(user);

            //Random random = new Random();
            //int randomNumber = random.Next(1000, 9999);
            //user.verificationCode = randomNumber;

            user.verificationCode = 9524;
            _dbContext.SaveChanges();
            UserFamily userFamily = new UserFamily()
            {
                ApplicationUserId = user.Id,
                 Name = $"{user.FirstName} {user.LastName}",
                 BirthDate = user.BirthDate,
                 Deleted =false,
                 Gender= user.Gender,
                 IsOwner =true
            };

            _dbContext.UserFamilies.Add(userFamily);
            _dbContext.SaveChanges();
            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumberConfirmed = false
                //TwoFactorEnabled = user.TwoFactorEnabled
                //RefreshToken = refreshToken.Token,
                //RefreshTokenExpiration = refreshToken.ExpiresOn
            };
        }
        //public async Task<AuthModel> EditeAccount(EditUserDto model ,string Id)
        //{
        //    var user = await _userManager.FindByIdAsync(Id);

           

            

          
        //}
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);

            if (user is null || !await _userManager.CheckPasswordAsync(user, (model.Password + "v%D9")) || user.Deleted)
            {
                authModel.Message = "UserName or Password is incorrect!"; 
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;   
            authModel.FirstName=user.FirstName;
            authModel.LastName=user.LastName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            //authModel.Roles = rolesList.ToList();
            authModel.PhoneNumber = user.PhoneNumber;
            //authModel.TwoFactorEnabled = user.TwoFactorEnabled;
            authModel.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            //if (user.RefreshTokens.Any(t => t.IsActive))
            //{
            //    var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            //    authModel.RefreshToken = activeRefreshToken.Token;
            //    authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            //}
            //else
            //{
            //    var refreshToken = GenerateRefreshToken();
            //    authModel.RefreshToken = refreshToken.Token;
            //    authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
            //    user.RefreshTokens.Add(refreshToken);
            //    await _userManager.UpdateAsync(user);
            //}

            return authModel;
        }
        public async Task<AuthModel> GetTokenAsync(string PhoneNumber)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByNameAsync(PhoneNumber);

         

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            //authModel.Roles = rolesList.ToList();
            authModel.PhoneNumber = user.PhoneNumber;
            //authModel.TwoFactorEnabled = user.TwoFactorEnabled;
            //if (user.RefreshTokens.Any(t => t.IsActive))
            //{
            //    var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            //    authModel.RefreshToken = activeRefreshToken.Token;
            //    authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            //}
            //else
            //{
            //    var refreshToken = GenerateRefreshToken();
            //    authModel.RefreshToken = refreshToken.Token;
            //    authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
            //    user.RefreshTokens.Add(refreshToken);
            //    await _userManager.UpdateAsync(user);
            //}

            return authModel;
        }
        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Sonething went wrong";
        }
        //public async Task<AuthModel> RefreshTokenAsync(string token)
        //{
        //    var authModel = new AuthModel();

        //    var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        //    if (user == null)
        //    {
        //        authModel.Message = "Invalid token";
        //        return authModel;
        //    }

        //    var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        //    if (!refreshToken.IsActive)
        //    {
        //        authModel.Message = "Inactive token";
        //        return authModel;
        //    }

        //    refreshToken.RevokedOn = DateTime.UtcNow;

        //    var newRefreshToken = GenerateRefreshToken();
        //    user.RefreshTokens.Add(newRefreshToken);
        //    await _userManager.UpdateAsync(user);

        //    var jwtToken = await CreateJwtToken(user);
        //    authModel.IsAuthenticated = true;
        //    authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        //    authModel.Email = user.Email;
        //    authModel.Username = user.UserName;
        //    var roles = await _userManager.GetRolesAsync(user);
        //    authModel.Roles = roles.ToList();
        //    authModel.RefreshToken = newRefreshToken.Token;
        //    authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

        //    return authModel;
        //}

        //public async Task<bool> RevokeTokenAsync(string token)
        //{
        //    var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        //    if (user == null)
        //        return false;

        //    var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        //    if (!refreshToken.IsActive)
        //        return false;

        //    refreshToken.RevokedOn = DateTime.UtcNow;

        //    await _userManager.UpdateAsync(user);

        //    return true;
        //}
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = new RNGCryptoServiceProvider();

            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}

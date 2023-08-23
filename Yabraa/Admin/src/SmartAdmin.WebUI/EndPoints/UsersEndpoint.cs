using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using SmartAdmin.WebUI.Data;
using SmartAdmin.WebUI.Extensions;
using SmartAdmin.WebUI.Models;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.EndPoints
{
    [ApiController]
    [Route("api/users")]
    public class UsersEndpoint : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly SmartSettings _settings;

        public UsersEndpoint(ApplicationDbContext context, UserManager<ApplicationUser> manager, SmartSettings settings)
        {
            _context = context;
            _manager = manager;
            _settings = settings;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ApplicationUserViewModel>>> Get()
        {
            // var users = await _manager.Users.Where(c=>c.IsSystemUser).AsNoTracking().ToListAsync();
            var users = await _manager.Users.Where(c => c.IsSystemUser).AsNoTracking().Select(c => new ApplicationUserViewModel { Id = c.Id, Email = c.Email, FirstName = c.FirstName, LastName = c.LastName, UserName = c.UserName,PhoneNumber=c.PhoneNumber }).ToListAsync();

            return Ok(new { data = users, recordsTotal = users.Count, recordsFiltered = users.Count });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApplicationUser>> Get([FromRoute]string id) => Ok(await _manager.FindByIdAsync(id));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromForm] ApplicationUser model)
        {
            //ApplicationUser user = new ApplicationUser();
            model.Id = Guid.NewGuid().ToString();
           //user.UserName = model.UserName ;
           //user.Email = model.Email;
           //user.PhoneNumber = model.PhoneNumber;
           //user.FirstName = model.FirstName;
           //user.LastName = model.LastName;
            var result = await _manager.CreateAsync(model);

            if (result.Succeeded)
            {
                // HACK: This password is just for demonstration purposes!
                // Please do NOT keep it as-is for your own project!
                result = await _manager.AddPasswordAsync(model, "password@#1");

                if (result.Succeeded)
                {
                    return CreatedAtAction("Get", new { id = model.Id }, model);
                }
            }
            //string ss = "";
            //if (!result.Succeeded)
            //{
            //    foreach (var item in result.Errors)
            //    {
            //        ss += $"{item.Description} ";
            //    }
            //}
            return BadRequest(result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update([FromForm]ApplicationUser model)
        {
            var result = await _context.UpdateAsync(model, model.Id);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromForm]ApplicationUser model)
        {
            // HACK: The code below is just for demonstration purposes!
            // Please use a different method of preventing the currently logged in user from being removed
            if (model.UserName == _settings.Theme.Email)
            {
                return BadRequest(SmartError.Failed("Please do not delete the main user! =)"));
            }

            var result = await _context.DeleteAsync<ApplicationUser>(model.Id);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result);
        }
    }
}

using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Services
{
    public class Service
    {
        public ApplicationDbContext _context { get; set; }
        private readonly IWebHostEnvironment _env;
        public Service(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<List<YabraaEF.Models.Service>> GetServices()
        {
            return await _context.Services.Where(c=> !c.Deleted && !c.ParentServiceId.HasValue ).Include(c=>c.ServiceType).ToListAsync();
        }
     
        public async Task<SelectList> GetServicesWithoutParents(int ServiceId = 0)
        {
            var Parents = _context.Services
                .Where(c => c.ParentServiceId.HasValue)
                .Select(c => c.ParentServiceId);
            var parents = await _context.Services.Where(c => !c.Deleted && !Parents.Contains(c.ServiceId)).ToListAsync();
            SelectList items;
            if (ServiceId > 0)
                items = new SelectList(parents, "ServiceId", "NameAR", ServiceId);
            else
                items = new SelectList(parents, "ServiceId", "NameAR");

            return items;
        }
       
        public async Task<SelectList> GetAvailableParentService(int ServiceId = 0)
        {
           
            var completeService = _context.Services
                .Where(c => c.ParentServiceId.HasValue && !c.Deleted)
                .GroupBy(p => p.ParentServiceId)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key.Value/*, count = g.Count()*/ );

           var parents = await _context.Services.Where(c => !c.Deleted && !c.ParentServiceId.HasValue && !completeService.Contains(c.ServiceId)).ToListAsync();
            SelectList items;
            if (ServiceId > 0)
                items = new SelectList(parents, "ServiceId", "NameAR", ServiceId);
            else
                items = new SelectList(parents, "ServiceId", "NameAR");

            return items;
        }

        public async Task<SelectList> GetServiceTypes(int ServiceTypeId = 0)
        {   

            var serviceTypes = await _context.ServiceTypes.Where(c => c.Enable).ToListAsync();
            SelectList items;
            if (ServiceTypeId > 0)
                items = new SelectList(serviceTypes, "ServiceTypeId", "Name", ServiceTypeId);
            else
                items = new SelectList(serviceTypes, "ServiceTypeId", "Name");

            return items;
        }
        public async Task<int> Create(ServicesCreateViewModel model)
        {
            //int temp = int.Parse(model.ParentServiceId);
            YabraaEF.Models.Service service = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = model.DetailsAR,
                DetailsEN = model.DetailsEN,
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                //ParentServiceId = model.ParentServiceId > 0 ? model.ParentServiceId : null,
                ParentServiceId = null,
                ServiceTypeId = model.ServiceTypeId
            };
            if (model.File != null && model.File.Length > 0)
            {
                var fileName = Path.GetFileName(model.File.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "servicesImages", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }             
                service.ImagePath = $"servicesImages/{fileName}";
            }
            _context.Services.Add(service);
            _context.SaveChanges();
            return service.ServiceId;

        }
        public async Task<ServicesCreateViewModel> GetServiceById(int ServiceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(c => c.ServiceId == ServiceId);
            if (service is not null)
            {
                ServicesCreateViewModel model = new ServicesCreateViewModel()
                {
                   DetailsAR = service.DetailsAR,
                   NameAR = service.NameAR,
                   NameEN = service.NameEN,
                   DetailsEN = service.DetailsEN,
                   //ParentServiceId = service.ParentServiceId.HasValue ? service.ParentServiceId.Value:0,
                   Path = service.ImagePath,
                   ServiceId = ServiceId,
                   ServiceTypeId = service.ServiceTypeId
                };
                return model;
            }
            return null;
        }
        public async Task<int?> Edit(ServicesCreateViewModel model)
        {
            var service = await _context.Services.FirstOrDefaultAsync(c => c.ServiceId == model.ServiceId);
            if (service is not null)
            {
                //service.ParentServiceId = model.ParentServiceId > 0 ? model.ParentServiceId : null;
                service.DetailsAR = model.DetailsAR;
                service.NameAR = model.NameAR;
                service.NameEN = model.NameEN;
                service.DetailsEN = model.DetailsEN;
                service.ServiceTypeId = model.ServiceTypeId;

                if (model.File != null && model.File.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.Path))
                    {
                        Photo.Delete(_env.WebRootPath, model.Path);
                    }
                    string fileName = await Photo.UploadAsync(model.File, "servicesImages");
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        service.ImagePath = fileName;
                    }

                }

                await _context.SaveChangesAsync();
                return service.ServiceId;
            }
            return null;

        }
        public async Task<bool> Delete(int ServiceId)
        {
            var service = await _context.Services.Where(c => c.ServiceId == ServiceId).Include(c => c.Categories).Include(c => c.Packages).FirstOrDefaultAsync();
            if (service is not null)
            {
                // Photo.Delete(_env.WebRootPath, service.ImagePath);
                service.Deleted = true;
                foreach (var category in service.Categories)
                {
                    category.Deleted = true;

                }
                foreach (var package in service.Packages)
                {
                    package.Deleted = true;

                }


                var childServices = _context.Services.Where(c => c.ParentServiceId == ServiceId).Include(c=>c.Categories).Include(c => c.Packages).ToList();
                if (childServices is not null && childServices.Count > 0)
                {
                    foreach (var child in childServices)
                    {
                        child.Deleted = true;
                        foreach (var category in child.Categories)
                        {
                            category.Deleted = true;

                        }
                        foreach (var package in child.Packages)
                        {
                            package.Deleted = true;

                        }
                    }
                }
               
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

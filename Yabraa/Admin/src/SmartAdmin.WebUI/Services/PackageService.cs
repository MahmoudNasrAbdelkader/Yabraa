using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Services
{
    public class PackageService
    {
        public ApplicationDbContext _context { get; set; }
        private readonly IWebHostEnvironment _env;
        public PackageService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }
        public async Task<List<PackageIndexViewModel>> GetPackages()
        {
            return await _context.Packages.Where(c=>!c.Deleted).OrderBy(c => c.ServiceId).Include(c => c.Service.ServiceType).Include(c=>c.Category)
                .Select( c=> new PackageIndexViewModel {ServiceType = c.Service.ServiceType.Name,   PackageId = c.PackageId,NameAR=c.NameAR,NameEN=c.NameEN,Category=c.Category.NameAR,Service=c.Service.NameAR }).ToListAsync();
        }
        public async Task<int> Create(PackageCreateViewModel model)
        {
            YabraaEF.Models.Package Package = new YabraaEF.Models.Package()
            {
                CreateDT = DateTime.Now,
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                ServiceId = model.ServiceId.Value,
                SubTitleAR = model.SubTitleAR,
                Price = model.Price.Value,
                DetailsAR = model.DetailsAR,
                DetailsEN = model.DetailsEN,
                CategoryId=model.CategoryId.Value,
                CreateSystemUserId="1",
                InstructionAR = model.InstructionAR,
                InstructionEN = model.InstructionEN,
                SubTitleEN = model.SubTitleEN,                 
            };
            if (model.File != null && model.File.Length > 0)
            {
                var fileName = Path.GetFileName(model.File.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "packageImages", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }
                Package.ImagePath = $"packageImages/{fileName}";
            }
            _context.Packages.Add(Package);
            await _context.SaveChangesAsync();
            return Package.CategoryId;

        }
        public async Task<PackageCreateViewModel> GetPackageById(int PackageId)
        {
            var package = await _context.Packages.FirstOrDefaultAsync(c => c.PackageId == PackageId);
            if (package is not null)
            {
                PackageCreateViewModel model = new PackageCreateViewModel()
                {
                   PackageId = package.PackageId,
                   Path= package.ImagePath,
                   CategoryId = package.CategoryId,
                   DetailsAR = package.DetailsAR,
                   DetailsEN = package.DetailsEN,
                   InstructionAR = package.InstructionAR,
                   NameAR = package.NameAR,
                   NameEN = package.NameEN,
                   InstructionEN = package.InstructionEN,
                   Price = package.Price,
                   ServiceId = package.ServiceId,
                   SubTitleAR = package.SubTitleAR,
                   SubTitleEN = package.SubTitleEN,                
                };
                return model;
            }
            return null;
        }
        public async Task<int?> Edit(PackageCreateViewModel model)
        {
            var package = await _context.Packages.FirstOrDefaultAsync(c => c.PackageId == model.PackageId.Value);

            if (package is not null)
            {
               package.SubTitleAR = model.SubTitleAR;
               package.SubTitleEN = model.SubTitleEN;
               package.NameEN = model.NameEN;
               package.NameAR = model.NameAR;
               package.InstructionAR = model.InstructionAR;
               package.InstructionEN = model.InstructionEN;
               package.DetailsAR = model.DetailsAR;
               package.DetailsEN = model.DetailsEN;
               package.CategoryId = model.CategoryId.Value;
               package.ServiceId = model.ServiceId.Value;
               package.Price = model.Price.Value;

                if (model.File != null && model.File.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(package.ImagePath))
                    {
                        Photo.Delete(_env.WebRootPath, package.ImagePath);
                    }
                    string fileName = await Photo.UploadAsync(model.File, "packageImages");
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        package.ImagePath = fileName;
                    }

                }

                await _context.SaveChangesAsync();
                return package.PackageId;
            }
            return null;

        }
        public async Task<bool> Delete(int PackageId)
        {
            var Package = await _context.Packages.FirstOrDefaultAsync(c => c.PackageId == PackageId);
            if (Package is not null)
            {
                //Photo.Delete(_env.WebRootPath, page.Path);                
                Package.Deleted = true;
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

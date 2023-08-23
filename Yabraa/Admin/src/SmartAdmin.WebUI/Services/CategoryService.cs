using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Services
{
    public class CategoryService
    {
        public ApplicationDbContext _context { get; set; }
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<YabraaEF.Models.Category>> GetCategories()
        {
            return await _context.Categories.Where(c => !c.Deleted).OrderBy(c => c.ServiceId).Include(c=>c.Service).ToListAsync();
        }
        public async Task<int> Create(CategoryCreateViewModel model)
        {
            YabraaEF.Models.Category Category = new YabraaEF.Models.Category()
            {
                CreateDT = DateTime.Now,
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                ServiceId = model.ServiceId.Value,
                Deleted = false
            };
           
            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();
            return Category.CategoryId;

        }
        public async Task<List<Category>> GetCategoriesByServiceId(int serviceId)
        {
           return await _context.Categories.Where(c => c.ServiceId == serviceId).ToListAsync();
        }
        public async Task<CategoryCreateViewModel> GetCategoryById(int CategoryId)
        {
            var Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == CategoryId);
            if (Category is not null)
            {
                CategoryCreateViewModel model = new CategoryCreateViewModel()
                {
                    CategoryId = CategoryId,
                    NameEN = Category.NameEN,
                    NameAR= Category.NameAR,
                    ServiceId = Category.ServiceId            
                };
                return model;
            }
            return null;
        }
        public async Task<int?> Edit(CategoryCreateViewModel model)
        {
            var Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId.Value);

            if (Category is not null)
            {
                Category.ServiceId = model.ServiceId.Value;
                Category.NameEN = model.NameEN;
                Category.NameAR = model.NameAR;

                await _context.SaveChangesAsync();
                return Category.CategoryId;
            }
            return null;

        }
        public async Task<bool> Delete(int CategoryId)
        {
            var Category = await _context.Categories.Where(c => c.CategoryId == CategoryId).Include(c=>c.Packages).FirstOrDefaultAsync();
            if (Category is not null)
            {
                Category.Deleted = true;
                foreach (var package in Category.Packages)
                {
                    package.Deleted = true;

                }
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Models;

namespace SmartAdmin.WebUI.Services
{
    public class StartPagesService
    {
        public ApplicationDbContext _context { get; set; }
        private readonly IWebHostEnvironment _env;
        public StartPagesService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }
        public async Task<List<StartPagesIndexViewModel>> GetstartPages()
        {
            return await _context.StartPages.OrderBy(c => c.OrderbyAscending).Select(c=> new StartPagesIndexViewModel {StartPageId =c.StartPageId,TitleAr=c.TitleAr,TitleEn=c.TitleEn }).ToListAsync();
        }

        public async Task<int> Create(StartPagesCreateViewModel model)
        {
            YabraaEF.Models.StartPages Page = new YabraaEF.Models.StartPages()
            {
               
                TitleEn = model.TitleEn,
                SubTitleEn = model.SubTitleEn,
                TitleAr = model.TitleAr,
                SubTitleAR = model.SubTitleAR,             
            };
            if (model.File != null && model.File.Length > 0)
            {
                string fileName = await Photo.UploadAsync(model.File, "startPagesImages");
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    Page.Path = fileName;
                }
            }
            _context.StartPages.Add(Page);
            await _context.SaveChangesAsync();
            return Page.StartPageId;

        }
        public async Task<StartPagesCreateViewModel> GetStartPageById(int StartPageId)
        {
           var page = await _context.StartPages.FirstOrDefaultAsync(c=>c.StartPageId == StartPageId);
            if (page is not null)
            {
                StartPagesCreateViewModel model = new StartPagesCreateViewModel() {
                    StartPageId=page.StartPageId,
                    SubTitleAR=page.SubTitleAR,
                    SubTitleEn=page.SubTitleEn,
                    TitleAr=page.TitleAr,
                    TitleEn=page.TitleEn,
                    Path = page.Path
                };
                return model;
            }
            return null;
        }
        public async Task<int?> Edit(StartPagesCreateViewModel model)
        {
            var page = await _context.StartPages.FirstOrDefaultAsync(c => c.StartPageId == model.StartPageId.Value);

            if (page is not null)
            {
                page.SubTitleAR = model.SubTitleAR;
                page.SubTitleEn = model.SubTitleEn;
                page.TitleEn = model.TitleEn;
                page.TitleAr=model.TitleAr;
                
                if (model.File != null && model.File.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.Path) )
                    {
                        Photo.Delete(_env.WebRootPath, model.Path);
                    }
                     string fileName = await Photo.UploadAsync(model.File, "startPagesImages");
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        page.Path = fileName;
                    }                  
                   
                }
                
                await _context.SaveChangesAsync();
                return page.StartPageId;
            }
            return null; 

        }
        public async Task<bool> Delete(int StartPageId)
        {
            var page = await _context.StartPages.FirstOrDefaultAsync(c => c.StartPageId == StartPageId);
            if (page is not null)
            {
                 Photo.Delete(_env.WebRootPath, page.Path);
                _context.StartPages.Remove(page);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

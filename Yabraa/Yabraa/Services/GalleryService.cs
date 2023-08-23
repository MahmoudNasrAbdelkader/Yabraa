
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using Yabraa.DTOs;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Services
{
    public class GalleryService
    {
        private ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public GalleryService(ApplicationDbContext dbContext, IConfiguration configuration)
        {

            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<List<string>> GetGalleryImages()
        {
            string url = _configuration.GetValue<string>("AdminUrl");
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            var images = await _dbContext.Gallery.OrderBy(c => c.OrderbyAscending).Select(c => $"{url}/{c.Path}").ToListAsync();
            List<string> data = new List<string>();
            if (images is not null && images.Count > 0)
            {
                foreach (var item in images)
                {
                    data.Add(item.Replace("\n", ""));
                }
            }
           
            return data;
        }

    }
}

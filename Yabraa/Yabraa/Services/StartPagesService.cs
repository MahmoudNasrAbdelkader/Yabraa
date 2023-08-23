using Microsoft.EntityFrameworkCore;
using Yabraa.DTOs;
using YabraaEF;

namespace Yabraa.Services
{
    public class StartPagesService
    {
        private ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public StartPagesService(ApplicationDbContext dbContext, IConfiguration configuration)
        {

            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<List<StartPagesDto>> GetStartPages()
        {
            string url = _configuration.GetValue<string>("AdminUrl");
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            var Pages = await _dbContext.StartPages.OrderBy(c => c.OrderbyAscending).
                Select(c => new StartPagesDto { Id =c.StartPageId, TitleAr = c.TitleAr, SubTitleAr = c.SubTitleAR, TitleEn =c.TitleEn,SubTitleEn =c.SubTitleEn,Path= $"{url}/{c.Path}" } ).ToListAsync();
            if (Pages is not null && Pages.Count > 0)
            {
                foreach (var item in Pages)
                {
                    item.Path = item.Path.Replace("\n", "");
                }
            }

            return Pages;
        }
       
    }
}

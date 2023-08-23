using YabraaEF;

namespace Yabraa.Services
{
    public class SharedService
    {
        private ApplicationDbContext _dbContext;

        public SharedService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
     
    }
}

using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository
{
    public class AppuserRepository : Repository<Appuser>, IAppuserRepository
    {
        private AppDbContext _db;
        public AppuserRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

    }
}

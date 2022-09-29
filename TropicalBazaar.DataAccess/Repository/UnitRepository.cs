using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository
{
    public class UnitRepository : Repository<Unit>, IUnitRepository
    {
        private AppDbContext _db;
        public UnitRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Unit obj)
        {
            _db.Units.Update(obj);
        }
    }
}

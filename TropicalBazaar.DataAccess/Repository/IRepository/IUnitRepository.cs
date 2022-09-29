using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository.IRepository
{
    public interface IUnitRepository : IRepository<Unit>
    {
        void Update(Unit obj);
    }
}

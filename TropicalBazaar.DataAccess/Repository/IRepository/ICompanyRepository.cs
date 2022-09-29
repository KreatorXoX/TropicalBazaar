using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
    }
}

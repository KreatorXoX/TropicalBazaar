using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}

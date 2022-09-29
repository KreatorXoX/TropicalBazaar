using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;

namespace TropicalBazaar.DataAccess.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private AppDbContext _db;
        public OrderDetailsRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetails obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}

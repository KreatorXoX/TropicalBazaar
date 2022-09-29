using System.Linq.Expressions;

namespace TropicalBazaar.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeForeignKeyProps = null);
        void Add(T entity);

        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeForeignKeyProps = null, bool tracked = true);

        // No update in generic repository because in product update we will have different logic.

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TropicalBazaar.DataAccess.Repository.IRepository;

namespace TropicalBazaar.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private AppDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(AppDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }


        public void Add(T entity)
        {
            dbSet.Add(entity);
        }


        //usage of includeForeignKeyProps = "Category" for multiple use commas "Category,ForeignKeyClass" 
        // its not only about including foreign key props we can include any model we want.
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeForeignKeyProps = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (includeForeignKeyProps != null)
            {
                foreach (var foreignKeyProp in includeForeignKeyProps.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(foreignKeyProp);
                }
            }

            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeForeignKeyProps = null, bool tracked = true)
        {
            IQueryable<T> query;

            if (tracked == true)
                query = dbSet;
            else
                query = dbSet.AsNoTracking();

            query = query.Where(filter);

            if (includeForeignKeyProps != null)
            {
                foreach (var foreignKeyProp in includeForeignKeyProps.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(foreignKeyProp);
                }
            }

            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}

using AmazeCareAPI.Contexts;
using AmazeCareAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public abstract class RepositoryDB<K, T> : IRepository<K, T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public RepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<T> Add(T entity)
        {
            _context.ChangeTracker.Clear();
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> Delete(K key)
        {
            _context.ChangeTracker.Clear();
            var obj = await GetById(key);
            _context.Remove(obj);
            await _context.SaveChangesAsync();
            return obj;
        }

        public abstract Task<IEnumerable<T>> GetAll();
        public abstract Task<T> GetById(K key);

        public virtual async Task<T> Update(K key, T entity)
        {
            _context.ChangeTracker.Clear();
            var obj = await GetById(key);
            _context.Entry<T>(obj).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }

}

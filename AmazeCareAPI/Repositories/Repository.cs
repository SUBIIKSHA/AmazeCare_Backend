using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace AmazeCareAPI.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected static List<T> list = new List<T>();

        public virtual async Task<T> Add(T entity)
        {
            list.Add(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<T> Delete(K key)
        {
            var item = await GetById(key);
            if (item != null)
            {
                list.Remove(item);
                return item;
            }
            throw new NoSuchEntityException();
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await Task.FromResult(list);
        }

        public abstract Task<T> GetById(K key);

        public virtual async Task<T> Update(K key, T entity)
        {
            var item = await GetById(key);
            if (item != null)
            {
                list.Remove(item);
                list.Add(entity);
                return entity;
            }
            throw new NoSuchEntityException();
        }
    }

}

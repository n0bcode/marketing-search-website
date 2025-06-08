using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Repositories.IRepositories
{
    public interface IRepositoryMongo<T> where T : class
    {
        Task AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
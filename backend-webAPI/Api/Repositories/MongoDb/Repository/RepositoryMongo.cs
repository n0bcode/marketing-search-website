using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using MongoDB.Driver;

namespace Api.Repositories.MongoDb
{
    public class RepositoryMongo<T> : IRepositoryMongo<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;

        public RepositoryMongo(MongoDbContext context, string collectionName)
        {
            var collection = context.GetType().GetProperty(collectionName)!.GetValue(context) as IMongoCollection<T>;
            _collection = collection ?? throw new InvalidOperationException($"Collection '{collectionName}' not found or is not of type IMongoCollection<{typeof(T).Name}>.");
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return await _collection.Find(_ => true).ToListAsync();
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task RemoveAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }

        public async Task RemoveRangeAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }
    }
}
using Ether.Interfaces;
using Ether.Types.Attributes;
using Ether.Types.Configuration;
using Ether.Types.DTO;
using Ether.Types.DTO.Reports;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Ether.Types.Data
{
    public class MongoRepository : IRepository
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoRepository(IOptions<DbConfig> dbConfig)
        {
            //TODO: Shouldn't be here
            BsonClassMap.RegisterClassMap<PullRequestsReport>();
            BsonClassMap.RegisterClassMap<WorkItemsReport>();

            _client = new MongoClient(dbConfig.Value.ConnectionString);
            _database = _client.GetDatabase(dbConfig.Value.DbName);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : BaseDto
        {
            return await GetCollectionFor<T>()
                .AsQueryable()
                .ToListAsync();
        }

        public async Task<IEnumerable> GetAllByTypeAsync(Type itemType)
        {
            var items = await GetCollectionFor(itemType)
                .AsQueryable()
                .ToListAsync();

            var castMethod = typeof(Enumerable)
                .GetMethod("Cast")
                .MakeGenericMethod(new[] { itemType });

            return (IEnumerable)castMethod.Invoke(null, new object[] { items
                .ToList()
                .Select(i => BsonSerializer.Deserialize(i, itemType)) });
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseDto
        {
             return await GetCollectionFor<T>()
                .AsQueryable()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseDto
        {
            return await GetCollectionFor<T>()
                .AsQueryable()
                .SingleOrDefaultAsync(predicate);
        }

        public async Task<object> GetSingleAsync(Guid id, Type itemType)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var item = await GetCollectionFor(itemType)
                .Find(filter)
                .SingleOrDefaultAsync();

            if (item == null)
                return null;

            return BsonSerializer.Deserialize(item, itemType);
        }

        public async Task<TProjection> GetFieldValue<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto
        {
            return await GetCollectionFor<TType>()
                  .Find(predicate)
                  .Project(projection)
                  .SingleOrDefaultAsync();
        }

        public async Task<bool> CreateAsync<T>(T item) where T : BaseDto
        {
            var collection = GetCollectionFor<T>();
            var count = await collection
               .AsQueryable()
               .CountAsync(i => i.Id == item.Id);

            if (count > 0)
                return false;

            await collection.InsertOneAsync(item);
            return true;
        }

        public async Task<bool> CreateOrUpdateAsync<T>(T item) where T : BaseDto
        {
            var collection = GetCollectionFor<T>();
            var count = await collection
               .AsQueryable()
               .CountAsync(i => i.Id == item.Id);

            if (count > 0)
            {
                await collection.FindOneAndReplaceAsync(i => i.Id == item.Id, item);
            }
            else
            {
                await collection.InsertOneAsync(item);
            }

            return true;
        }

        public async Task<bool> DeleteAsync<T>(Guid id) where T : BaseDto
        {
            await GetCollectionFor<T>().FindOneAndDeleteAsync(i => i.Id == id);
            return true;
        }

        private string GetCollectionNameFor(Type type)
        {
            var dbNameAttr = type
                .GetTypeInfo()
                .GetCustomAttribute<DbNameAttribute>();

            return dbNameAttr != null ? dbNameAttr.Name : type.Name;
        }

        private IMongoCollection<T> GetCollectionFor<T>()
             where T : BaseDto
        {
            var collectionName = GetCollectionNameFor(typeof(T));
            return _database.GetCollection<T>(collectionName);
        }

        private IMongoCollection<BsonDocument> GetCollectionFor(Type type)
        {
            var collectionName = GetCollectionNameFor(type);
            return _database.GetCollection<BsonDocument>(collectionName);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Ether.Contracts.Attributes;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Ether.Core.Data
{
    public class MongoRepository : IRepository
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoRepository(IOptions<DbConfiguration> dbConfig, IDbConfigurator dbConfigurator)
        {
            // TODO: Shouldn't be here

            foreach (var registration in dbConfigurator.Registrations)
            {
                SafeRegisterClassMap(registration.TypeToRegister);
            }

            // SafeRegisterClassMap<PullRequestsReport>();
            // SafeRegisterClassMap<WorkItemsReport>();
            // SafeRegisterClassMap<ListOfReviewersReport>();
            // SafeRegisterClassMap<AggregatedWorkitemsETAReport>();
            var pack = new ConventionPack();
            pack.Add(new IgnoreExtraElementsConvention(true));

            ConventionRegistry.Register("My Custom Conventions", pack, t => t.FullName.StartsWith("Ether."));
            _client = new MongoClient(dbConfig.Value.ConnectionString);
            _database = _client.GetDatabase(dbConfig.Value.DbName);

            foreach (var indexDescriptor in dbConfigurator.Indexes)
            {
                var typeCollection = GetCollectionFor(indexDescriptor.DocumentType);
                var indexDefinition = indexDescriptor.IsAscending ?
                    Builders<BsonDocument>.IndexKeys.Ascending(indexDescriptor.Field) :
                    Builders<BsonDocument>.IndexKeys.Descending(indexDescriptor.Field);
#pragma warning disable CS0618 // Type or member is obsolete
                typeCollection.Indexes.CreateOne(indexDefinition, new CreateIndexOptions { Unique = true });
#pragma warning restore CS0618 // Type or member is obsolete
            }

            /*
            var vstsWorkItemsCollection = GetCollectionFor<VSTSWorkItem>();
            vstsWorkItemsCollection.Indexes.CreateOne(Builders<VSTSWorkItem>.IndexKeys.Ascending(w => w.WorkItemId), new CreateIndexOptions
            {
                Unique = true
            });

            var pullREquestsCollection = GetCollectionFor<PullRequest>();
            pullREquestsCollection.Indexes.CreateOne(Builders<PullRequest>.IndexKeys.Ascending(w => w.PullRequestId), new CreateIndexOptions
            {
                Unique = true
            });
            */
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>()
            where T : BaseDto
        {
            return await GetCollectionFor<T>()
                .AsQueryable()
                .ToListAsync();
        }

        public IEnumerable<T> GetAll<T>()
            where T : BaseDto
        {
            return GetCollectionFor<T>()
                .AsQueryable()
                .ToList();
        }

        public async Task<IEnumerable> GetAllByTypeAsync(Type itemType)
        {
            var items = await GetCollectionFor(itemType)
                .AsQueryable()
                .ToListAsync();

            var castMethod = typeof(Enumerable)
                .GetMethod("Cast")
                .MakeGenericMethod(new[] { itemType });

            return (IEnumerable)castMethod.Invoke(null, new object[]
            {
                items
                    .ToList()
                    .Select(i => BsonSerializer.Deserialize(i, itemType))
            });
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto
        {
            return await GetCollectionFor<T>()
               .AsQueryable()
               .Where(predicate)
               .ToListAsync();
        }

        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto
        {
            return GetCollectionFor<T>()
               .AsQueryable()
               .Where(predicate)
               .ToList();
        }

        public async Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto
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
            {
                return null;
            }

            return BsonSerializer.Deserialize(item, itemType);
        }

        public async Task<T> GetSingleAsync<T>(Guid id)
            where T : BaseDto
        {
            return (T)await GetSingleAsync(id, typeof(T));
        }

        public async Task<TProjection> GetFieldValueAsync<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto
        {
            return await GetCollectionFor<TType>()
                  .Find(predicate)
                  .Project(projection)
                  .SingleOrDefaultAsync();
        }

        public TProjection GetFieldValue<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto
        {
            return GetCollectionFor<TType>()
                  .Find(predicate)
                  .Project(projection)
                  .SingleOrDefault();
        }

        public async Task UpdateFieldValue<T, TField>(T obj, Expression<Func<T, TField>> field, TField value)
            where T : BaseDto
        {
            var collection = GetCollectionFor<T>();
            var update = Builders<T>.Update.Set(field, value);
            await collection.UpdateOneAsync<T>(e => e.Id == obj.Id, update);
        }

        public async Task<bool> CreateAsync<T>(T item)
            where T : BaseDto
        {
            var collection = GetCollectionFor<T>();
            var count = await collection
               .AsQueryable()
               .CountAsync(i => i.Id == item.Id);

            if (count > 0)
            {
                return false;
            }

            await collection.InsertOneAsync(item);
            return true;
        }

        public async Task<bool> CreateOrUpdateAsync<T>(T item)
            where T : BaseDto
        {
            return await CreateOrUpdateAsync(item, i => i.Id == item.Id);
        }

        public async Task<bool> CreateOrUpdateAsync<T>(T item, Expression<Func<T, bool>> criteria)
            where T : BaseDto
        {
            var collection = GetCollectionFor<T>();
            var existingId = await GetFieldValueAsync(criteria, o => o.Id);
            if (existingId != Guid.Empty)
            {
                item.Id = existingId;
                await collection.ReplaceOneAsync(criteria, item);
            }
            else
            {
                await collection.InsertOneAsync(item);
            }

            return true;
        }

        public async Task<bool> DeleteAsync<T>(Guid id)
            where T : BaseDto
        {
            await GetCollectionFor<T>().FindOneAndDeleteAsync(i => i.Id == id);
            return true;
        }

        public long Delete<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto
        {
            var result = GetCollectionFor<T>()
                .DeleteMany(predicate);

            return result.DeletedCount;
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

        private void SafeRegisterClassMap(Type type)
        {
            if (!BsonClassMap.IsClassMapRegistered(type))
            {
                BsonClassMap.RegisterClassMap(new BsonClassMap(type));
            }
        }
    }
}

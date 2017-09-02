using Ether.Interfaces;
using Ether.Types.DTO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.Data
{
    public class FileRepository : IRepository
    {
        private string _dataPath = "";

        public FileRepository(IHostingEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(_dataPath))
                Directory.CreateDirectory(_dataPath);
        }

        public async Task<bool> CreateAsync<T>(T item, Type typeOverride = null)
            where T : BaseDto
        {
            var type = typeOverride == null ? typeof(T) : typeOverride;
            var records = (await GetAllByTypeAsync(type))
                .OfType<T>()
                .ToList();
            records.Add(item);
            await SaveAsync(records, typeOverride);

            return true;
        }

        public async Task<bool> DeleteAsync<T>(Guid id)
            where T : BaseDto
        {
            var records = (await GetAllAsync<T>()).ToList();
            var itemToDelete = records.SingleOrDefault(i => i.Id == id);
            var isSuccess = itemToDelete!=null && records.Remove(itemToDelete);
            if (isSuccess)
                await SaveAsync(records);

            return isSuccess;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Func<T, bool> predicate) 
            where T : BaseDto
        {
            return (await GetAllAsync<T>()).Where(predicate);
        }

        public async Task<T> GetSingleAsync<T>(Func<T, bool> predicate, Func<T, Type> typeOverride = null) 
            where T : BaseDto
        {
            var allItems = await GetAllAsync<T>();
            var item = allItems.FirstOrDefault(predicate);

            //TODO: This is ugly, need a better way
            if (typeOverride == null)
                return item;

            var type = typeOverride(item);
            var newItems = await GetAllByTypeAsync(typeof(T), type);
            return newItems
                .Cast<T>()
                .FirstOrDefault(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() 
            where T : BaseDto
        {
            var file = GetFilePath<T>();
            if (!File.Exists(file))
                return Enumerable.Empty<T>();

            var text = await File.ReadAllTextAsync(file);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(text);
        }

        public async Task<IEnumerable> GetAllByTypeAsync(Type itemType, Type typeOverride = null)
        {
            var file = GetFilePath(itemType);
            if (!File.Exists(file))
                return Enumerable.Empty<BaseDto>();

            var text = await File.ReadAllTextAsync(file);
            var generic = typeof(IEnumerable<>);
            var actualType = typeOverride == null ? itemType : typeOverride;
            var genericCollection = generic.MakeGenericType(actualType);
            return JsonConvert.DeserializeObject(text, genericCollection) as IEnumerable;
        }

        public async Task<bool> CreateOrUpdateAsync<T>(T item) 
            where T : BaseDto
        {
            var records = (await GetAllAsync<T>()).ToList();
            var idx = records.IndexOf(item);
            if (idx == -1)
                return await CreateAsync(item);

            records.RemoveAt(idx);
            records.Insert(idx, item);
            await SaveAsync(records);

            return true;
        }

        private async Task SaveAsync<T>(IEnumerable<T> items, Type typeOverride = null)
        {
            var type = typeOverride == null ? typeof(T) : typeOverride;
            var file = GetFilePath(type);
            await File.WriteAllTextAsync(file, JsonConvert.SerializeObject(items));
        }

        private string GetFilePath<T>()
        {
            return GetFilePath(typeof(T));
        }

        private string GetFilePath(Type itemType)
        {
            var typeName = itemType.Name;
            return Path.Combine(_dataPath, $"{typeName}.json");
        }
    }
}

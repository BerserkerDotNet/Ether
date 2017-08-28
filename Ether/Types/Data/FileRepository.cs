using System;
using System.Collections.Generic;
using Ether.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Ether.Types.DTO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;

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

        public async Task<bool> CreateAsync<T>(T item)
            where T : BaseDto
        {
            var records = (await GetAllAsync<T>()).ToList();
            records.Add(item);
            await SaveAsync(records);

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

        public async Task<T> GetSingleAsync<T>(Func<T, bool> predicate) 
            where T : BaseDto
        {
            return (await GetAllAsync<T>()).FirstOrDefault(predicate);
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

        public IEnumerable GetAll(Type itemType)
        {
            var file = GetFilePath(itemType);
            if (!File.Exists(file))
                return Enumerable.Empty<BaseDto>();

            var text = File.ReadAllText(file);
            var generic = typeof(IEnumerable<>);
            var genericCollection = generic.MakeGenericType(itemType);
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

        private async Task SaveAsync<T>(IEnumerable<T> items)
        {
            var file = GetFilePath<T>();
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

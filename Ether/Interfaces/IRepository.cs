using Ether.Types.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Interfaces
{
    public interface IRepository
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T: BaseDto;

        IEnumerable GetAll(Type itemType);

        Task<IEnumerable<T>> GetAsync<T>(Func<T, bool> predicate) where T : BaseDto;

        Task<T> GetSingleAsync<T>(Func<T, bool> predicate) where T : BaseDto;

        Task<bool> CreateAsync<T>(T item) where T : BaseDto;

        Task<bool> CreateOrUpdateAsync<T>(T item) where T : BaseDto;

        Task<bool> DeleteAsync<T>(Guid id) where T : BaseDto;
    }
}

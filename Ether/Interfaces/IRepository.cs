using Ether.Types.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ether.Interfaces
{
    public interface IRepository
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T: BaseDto;

        Task<IEnumerable> GetAllByTypeAsync(Type itemType);

        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseDto;

        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseDto;

        Task<TProjection> GetFieldValue<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection) where TType : BaseDto;

        Task<object> GetSingleAsync(Guid id, Type itemType);

        Task<bool> CreateAsync<T>(T item) where T : BaseDto;

        Task<bool> CreateOrUpdateAsync<T>(T item) where T : BaseDto;

        Task<bool> DeleteAsync<T>(Guid id) where T : BaseDto;
    }
}

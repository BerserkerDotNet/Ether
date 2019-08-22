using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;

namespace Ether.Contracts.Interfaces
{
    public interface IRepository
    {
        Task<IEnumerable<T>> GetAllAsync<T>()
            where T : BaseDto;

        Task<IEnumerable<T>> GetAllPagedAsync<T>(int page = 1, int itemsPerPage = 10)
            where T : BaseDto;

        IEnumerable<T> GetAll<T>()
            where T : BaseDto;

        Task<IEnumerable> GetAllByTypeAsync(Type itemType);

        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto;

        IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto;

        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto;

        T GetSingle<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto;

        Task<TProjection> GetFieldValueAsync<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto;

        Task<TProjection> GetFieldValueAsync<TType, TProjection>(Guid id, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto;

        Task<TResult> GetFieldValueAsync<TType, TProjection, TResult>(Guid id, Expression<Func<TType, TProjection>> projection)
             where TType : BaseDto;

        TProjection GetFieldValue<TType, TProjection>(Expression<Func<TType, bool>> predicate, Expression<Func<TType, TProjection>> projection)
            where TType : BaseDto;

        Task UpdateFieldValue<T, TField>(T obj, Expression<Func<T, TField>> field, TField value)
            where T : BaseDto;

        Task UpdateFieldValue<T, TField>(Expression<Func<T, bool>> filter, Expression<Func<T, TField>> field, TField value)
            where T : BaseDto;

        Task<object> GetSingleAsync(Guid id, Type itemType);

        Task<T> GetSingleAsync<T>(Guid id)
            where T : BaseDto;

        Task<bool> CreateAsync<T>(T item)
            where T : BaseDto;

        Task<bool> CreateOrUpdateIfAsync<T>(Expression<Func<T, bool>> criteria, T item)
            where T : BaseDto;

        Task<bool> CreateOrUpdateAsync<T>(T item)
            where T : BaseDto;

        Task<bool> CreateOrUpdateAsync<T>(T item, Expression<Func<T, bool>> criteria)
            where T : BaseDto;

        Task<bool> DeleteAsync<T>(Guid id)
            where T : BaseDto;

        long Delete<T>(Expression<Func<T, bool>> predicate)
            where T : BaseDto;

        Task<long> CountAsync<T>()
            where T : BaseDto;
    }
}

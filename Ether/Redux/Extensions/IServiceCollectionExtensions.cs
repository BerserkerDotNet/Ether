using System;
using System.Linq.Expressions;
using Ether.Redux.Interfaces;
using Ether.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Ether.Redux.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddReduxStore<TRootState>(this IServiceCollection services, Action<ReduxStoreConfig<TRootState>> cfg)
            where TRootState : new()
        {
            var reducerMapping = ReducerMappingBuilder<TRootState>.Create();
            var config = new ReduxStoreConfig<TRootState>(services, reducerMapping);
            cfg(config);
            var rootReducer = reducerMapping.Build();
            services.AddSingleton<IStore<TRootState>>(s => new Store<TRootState>(rootReducer, new BlazorActionResolver(s), s.GetService<LocalStorage>()));
        }
    }

    public class ReduxStoreConfig<TRootState>
         where TRootState : new()
    {
        private readonly IServiceCollection _services;
        private readonly ReducerMappingBuilder<TRootState> _reducerMapper;

        public ReduxStoreConfig(IServiceCollection services, ReducerMappingBuilder<TRootState> reducerMapper)
        {
            _services = services;
            _reducerMapper = reducerMapper;
        }

        public void Map<TProperty>(Expression<Func<TRootState, TProperty>> property, IReducer<TProperty> reducer)
        {
            _reducerMapper.Map(property, reducer);
        }

        public void RegisterAsyncAction<TAsyncAction, TProperty>()
            where TAsyncAction : class, IAsyncAction<TProperty>
        {
            _services.AddTransient<TAsyncAction>();
        }
    }
}

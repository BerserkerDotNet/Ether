using System;
using Ether.Redux.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ether.Redux.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddReduxStore<TRootState>(this IServiceCollection services, Action<ReducerMappingBuilder<TRootState>> cfg)
            where TRootState : new()
        {
            var reducerMapping = ReducerMappingBuilder<TRootState>.Create();
            cfg(reducerMapping);
            var rootReducer = reducerMapping.Build();
            services.AddSingleton<IStore<TRootState>>(new Store<TRootState>(rootReducer));
        }
    }
}

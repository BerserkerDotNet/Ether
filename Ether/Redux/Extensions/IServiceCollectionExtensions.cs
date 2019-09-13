using System;
using System.Linq;
using System.Linq.Expressions;
using Blazor.Extensions.Storage;
using Ether.Redux.Blazor;
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
            services.AddSingleton<IStore<TRootState>>(s => new Store<TRootState>(rootReducer, new BlazorActionResolver(s), s.GetService<LocalStorage>(), s.GetService<ReduxDevToolsInterop>()));
            services.AddSingleton<ReduxDevToolsInterop>();
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

        public void RegisterAsyncAction<TAsyncAction>()
            where TAsyncAction : class, IAsyncAction
        {
            _services.AddTransient<TAsyncAction>();
        }

        public void RegisterActionFromAssembly<TAsyncAction>()
            where TAsyncAction : class, IAsyncAction
        {
            var actionType = typeof(IAsyncAction);
            var actionTypeGeneric = typeof(IAsyncAction<>);
            var asyncActions = typeof(TAsyncAction).Assembly
                .GetTypes()
                .Where(t => t.IsClass && (actionType.IsAssignableFrom(t) || IsAssignableToGenericType(t, actionTypeGeneric)))
                .ToArray();

            foreach (var action in asyncActions)
            {
                _services.AddTransient(action);
            }
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = givenType.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}

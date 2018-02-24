using Ether.Core.Interfaces;
using System;

namespace Ether.Services
{
    public class AspNetDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _provider;

        public AspNetDependencyResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T Resolve<T>()
            where T: class
        {
            var type = typeof(T);
            return _provider.GetService(type) as T;
        }
    }
}

using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ether.Core.Proxy
{
    public class ProxyHook : IProxyGenerationHook
    {
        readonly IEnumerable<Type> _supportedTypes;

        public ProxyHook(IEnumerable<Type> supportedTypes)
        {
            _supportedTypes = supportedTypes;
        }

        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return _supportedTypes.Contains(type);
        }
    }
}

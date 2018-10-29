using System;

namespace Ether.Contracts.Types
{
    public class ClassMapRegistration
    {
        public ClassMapRegistration(Type typeToRegister)
        {
            TypeToRegister = TypeToRegister;
        }

        public Type TypeToRegister { get; }
    }
}

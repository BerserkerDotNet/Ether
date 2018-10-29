using System;

namespace Ether.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DbNameAttribute : Attribute
    {
        public DbNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

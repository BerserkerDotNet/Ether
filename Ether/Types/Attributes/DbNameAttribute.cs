using System;

namespace Ether.Types.Attributes
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DbNameAttribute : Attribute
    {
        private readonly string _name;

        public DbNameAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}

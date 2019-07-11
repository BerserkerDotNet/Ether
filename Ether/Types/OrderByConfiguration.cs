using System;

namespace Ether.Types
{
    public class OrderByConfiguration<T>
    {
        public OrderByConfiguration(Func<T, object> property, bool isDesc)
        {
            Property = property;
            IsDescending = isDesc;
        }

        public Func<T, object> Property { get; set; }

        public bool IsDescending { get; set; }
    }
}

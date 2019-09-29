using System;
using System.Collections.Generic;

namespace Ether.Types
{
    public class OrderByConfigurationBuilder<T>
    {
        private List<OrderByConfiguration<T>> _config = new List<OrderByConfiguration<T>>(3);

        public static OrderByConfigurationBuilder<T> New()
        {
            return new OrderByConfigurationBuilder<T>();
        }

        public OrderByConfigurationBuilder<T> OrderBy(Func<T, object> order)
        {
            _config.Add(new OrderByConfiguration<T>(order, isDesc: false));
            return this;
        }

        public OrderByConfigurationBuilder<T> OrderByDescending(Func<T, object> order)
        {
            _config.Add(new OrderByConfiguration<T>(order, isDesc: true));
            return this;
        }

        public OrderByConfiguration<T>[] Build()
        {
            return _config.ToArray();
        }
    }
}

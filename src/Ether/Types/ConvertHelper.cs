using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ether.Types
{
    public static class ConvertHelper
    {
        private static Dictionary<Type, TypeConverter> _convertersCache = new Dictionary<Type, TypeConverter>();

        public static TResult ConvertTo<TResult>(object value)
        {
            var converter = GetConverter<TResult>();
            return (TResult)converter.ConvertFrom(value);
        }

        private static TypeConverter GetConverter<TResult>()
        {
            var destinationType = typeof(TResult);
            if (!_convertersCache.ContainsKey(destinationType))
            {
                _convertersCache.Add(destinationType, TypeDescriptor.GetConverter(destinationType));
            }

            return _convertersCache[destinationType];
        }
    }
}

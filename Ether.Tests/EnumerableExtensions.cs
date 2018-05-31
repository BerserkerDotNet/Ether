using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Tests
{
    public static class EnumerableExtensions
    {
        private static Random _random;

        static EnumerableExtensions()
        {
            _random = new Random();
        }
        public static TSource Random<TSource>(this IEnumerable<TSource> source)
        {
            return source.ElementAt(_random.Next(source.Count()));
        }
    }
}

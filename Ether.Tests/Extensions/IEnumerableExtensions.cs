using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Tests.Extensions
{
    public static class IEnumerableExtensions
    {
        private static Random _random = new Random();

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> values, int min, int max)
        {
            return PickRandom(values, _random.Next(min, max));
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> values, int count = 1)
        {
            var total = values.Count() - 1;
            return Enumerable.Range(0, count).Select(_ => values.ElementAt(_random.Next(0, total))).ToArray();
        }

        public static T PickRandom<T>(this IEnumerable<T> values)
        {
            return PickRandom(values, 1).Single();
        }
    }
}

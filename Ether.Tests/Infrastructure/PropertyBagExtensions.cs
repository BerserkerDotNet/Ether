using System.Reflection;
using NUnit.Framework.Interfaces;
using static NUnit.Framework.TestContext;

namespace Ether.Tests.Infrastructure
{
    public static class PropertyBagExtensions
    {
        public static T Get<T>(this IPropertyBag properties, string key, T defaultValue = default(T))
        {
            var result = properties.Get(key);
            if (result != null)
                return (T)result;

            return defaultValue;
        }

        public static T Get<T>(this TestAdapter adapter, string key, T defaultValue = default(T))
        {
            if (adapter.Properties.ContainsKey(key))
                return adapter.Properties.Get<T>(key, defaultValue);

            return GetDeep(adapter, key, defaultValue);
        }

        private static T GetDeep<T>(object test, string key, T defaultValue)
        {
            if (test == null)
                return defaultValue;

            var testField = test.GetType().GetField("_test", BindingFlags.NonPublic | BindingFlags.Instance);
            var testValue = testField.GetValue(test);
            var parentField = testValue.GetType().GetProperty("Parent", BindingFlags.Public | BindingFlags.Instance);
            var parentValue = parentField.GetValue(testValue);
            var propertiesField = parentValue.GetType().GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance);
            var propertiesValue = propertiesField.GetValue(parentValue) as IPropertyBag;

            return propertiesValue.Get<T>(key, defaultValue);
        }
    }


}

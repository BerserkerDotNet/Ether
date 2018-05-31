using System.Reflection;
using NUnit.Framework.Interfaces;
using static NUnit.Framework.TestContext;

namespace Ether.Tests.Infrastructure
{
    public static class PropertyBagExtensions
    {
        public static int GetInt(this IPropertyBag properties, string key, int defaultValue = 0)
        {
            var result = properties.Get(key);
            if (result != null)
                return (int)result;

            return defaultValue;
        }

        public static int GetInt(this TestAdapter adapter, string key, int defaultValue = 0)
        {
            if (adapter.Properties.ContainsKey(key))
                return adapter.Properties.GetInt(key, defaultValue);

            return GetDeep(adapter, key, defaultValue);
        }

        private static int GetDeep(object test, string key, int defaultValue)
        {
            if (test == null)
                return defaultValue;

            var testField = test.GetType().GetField("_test", BindingFlags.NonPublic | BindingFlags.Instance);
            var testValue = testField.GetValue(test);
            var parentField = testValue.GetType().GetProperty("Parent", BindingFlags.Public | BindingFlags.Instance);
            var parentValue = parentField.GetValue(testValue);
            var propertiesField = parentValue.GetType().GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance);
            var propertiesValue = propertiesField.GetValue(parentValue) as IPropertyBag;

            return propertiesValue.GetInt(key, defaultValue);
        }
    }


}

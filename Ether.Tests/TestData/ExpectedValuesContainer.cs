using System.Collections.Generic;

namespace Ether.Tests.TestData
{
    public class ExpectedValuesContainer
    {
        private readonly Dictionary<string, object> _values;

        public ExpectedValuesContainer(Dictionary<string, object> values)
        {
            _values = values;
        }

        public T GetValue<T>(string name) => (T)_values[name];
    }
}

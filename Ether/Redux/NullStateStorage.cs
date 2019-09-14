using System.Threading.Tasks;
using Ether.Redux.Interfaces;

namespace Ether.Redux
{
    public class NullStateStorage : IStateStorage
    {
        public ValueTask<T> Get<T>(string key)
        {
            return new ValueTask<T>(Task.FromResult<T>(default));
        }

        public ValueTask Save<T>(string key, T state)
        {
            return new ValueTask(Task.CompletedTask);
        }
    }
}

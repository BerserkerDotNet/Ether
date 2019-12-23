using System.Threading.Tasks;

namespace Ether.Storage
{
    public interface ILocalStorage
    {
        ValueTask SetItem<T>(string key, T value);

        ValueTask<T> GetItem<T>(string key);
    }
}

using System.Threading.Tasks;

namespace Ether.Types.Interfaces
{
    public interface IRichTable<T>
    {
        void Edit(T item);
    }
}

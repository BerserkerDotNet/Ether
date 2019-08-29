using System.Threading.Tasks;

namespace Ether.Redux.Interfaces
{
    public interface IAsyncAction<TProperty>
    {
        Task Execute(IDispatcher dispatcher, TProperty property);
    }
}

using System.Threading.Tasks;

namespace Ether.Redux.Interfaces
{
    public interface IDispatcher
    {
        void Dispatch(IAction action);

        Task Dispatch<TAsyncAction, TProperty>(TProperty property)
            where TAsyncAction : IAsyncAction<TProperty>;
    }
}

using Ether.Contracts.Types;

namespace Ether.Contracts.Interfaces.CQS
{
    public interface ICommand<TResult>
    {
    }

    public interface ICommand : ICommand<UnitType>
    {
    }
}

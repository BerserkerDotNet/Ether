using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces.CQS
{
    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> Handle(TCommand command);
    }

    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task Handle(TCommand command);
    }
}

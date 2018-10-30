using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces.CQS
{
    public interface IMediator
    {
        Task<TResult> Request<TQuery, TResult>(IQuery<TResult> query)
            where TQuery : IQuery<TResult>;

        Task Execute<TCommand>(TCommand command)
            where TCommand : ICommand;

        Task<TResult> Execute<TCommand, TResult>(ICommand<TResult> command)
            where TCommand : ICommand<TResult>;
    }
}

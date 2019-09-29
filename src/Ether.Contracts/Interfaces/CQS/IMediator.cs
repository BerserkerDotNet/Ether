using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Contracts.Interfaces.CQS
{
    public interface IMediator
    {
        Task<TResult> Request<TQuery, TResult>(IQuery<TResult> query)
            where TQuery : IQuery<TResult>;

        Task<TResult> Request<TQuery, TResult>()
            where TQuery : IQuery<TResult>, new();

        Task<IEnumerable<TResult>> RequestCollection<TQuery, TResult>(IQuery<IEnumerable<TResult>> query)
            where TQuery : IQuery<IEnumerable<TResult>>;

        Task<PageViewModel<TResult>> RequestPagedCollection<TQuery, TResult>(IQuery<PageViewModel<TResult>> query)
             where TQuery : IQuery<PageViewModel<TResult>>;

        Task<IEnumerable<TResult>> RequestCollection<TQuery, TResult>()
            where TQuery : IQuery<IEnumerable<TResult>>, new();

        Task Execute<TCommand>(TCommand command)
            where TCommand : ICommand;

        Task<TResult> Execute<TResult>(ICommand<TResult> command);
    }
}

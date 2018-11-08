using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces.CQS
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query);
    }
}

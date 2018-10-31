using System;
using System.Threading.Tasks;
using Autofac;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;

namespace Ether.Core.Types
{
    public class DefaultMediator : IMediator
    {
        private readonly IComponentContext _context;

        public DefaultMediator(IComponentContext context)
        {
            _context = context;
        }

        public Task Execute<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            return Execute<TCommand, UnitType>(command);
        }

        public Task<TResult> Execute<TCommand, TResult>(ICommand<TResult> command)
            where TCommand : ICommand<TResult>
        {
            var handler = _context.Resolve<ICommandHandler<TCommand, TResult>>();
            if (handler == null)
            {
                throw new NotSupportedException($"No handler registered for command '{typeof(TCommand)}' with return type of '{typeof(TResult)}'");
            }

            return handler.Handle((TCommand)command);
        }

        public Task<TResult> Request<TQuery, TResult>(IQuery<TResult> query)
            where TQuery : IQuery<TResult>
        {
            var handler = _context.Resolve<IQueryHandler<TQuery, TResult>>();
            if (handler == null)
            {
                throw new NotSupportedException($"No handler registered for query '{typeof(TQuery)}' with return type of '{typeof(TResult)}'");
            }

            return handler.Handle((TQuery)query);
        }
    }
}

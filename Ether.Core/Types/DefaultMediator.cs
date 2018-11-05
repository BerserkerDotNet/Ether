using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Autofac;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types
{
    public class DefaultMediator : IMediator
    {
        private readonly IComponentContext _context;
        private readonly ILogger<DefaultMediator> _logger;

        public DefaultMediator(IComponentContext context, ILogger<DefaultMediator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Execute<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            try
            {
                _logger.LogInformation($"Executing {typeof(TCommand).Name} command.");
                var handler = _context.Resolve<ICommandHandler<TCommand>>();
                if (handler == null)
                {
                    throw new NotSupportedException($"No handler registered for command '{typeof(TCommand)}'");
                }

                await handler.Handle(command);
                _logger.LogInformation($"Executed {typeof(TCommand).Name} command.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {typeof(TCommand).Name} command.");
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }

        public async Task<TResult> Execute<TCommand, TResult>(ICommand<TResult> command)
            where TCommand : ICommand<TResult>
        {
            try
            {
                _logger.LogInformation($"Executing {typeof(TCommand).Name} command.");
                var handler = _context.Resolve<ICommandHandler<TCommand, TResult>>();
                if (handler == null)
                {
                    throw new NotSupportedException($"No handler registered for command '{typeof(TCommand)}' with return type of '{typeof(TResult)}'");
                }

                var result = await handler.Handle((TCommand)command);
                _logger.LogInformation($"Executed {typeof(TCommand).Name} command.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {typeof(TCommand).Name} command.");
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }

        public async Task<TResult> Request<TQuery, TResult>(IQuery<TResult> query)
            where TQuery : IQuery<TResult>
        {
            try
            {
                _logger.LogInformation($"Executing {typeof(TQuery).Name} query.");
                var handler = _context.Resolve<IQueryHandler<TQuery, TResult>>();
                if (handler == null)
                {
                    throw new NotSupportedException($"No handler registered for query '{typeof(TQuery)}' with return type of '{typeof(TResult)}'");
                }

                var result = await handler.Handle((TQuery)query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {typeof(TQuery).Name} query.");
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }

        public Task<TResult> Request<TQuery, TResult>()
            where TQuery : IQuery<TResult>, new()
        {
            return Request<TQuery, TResult>(new TQuery());
        }

        public Task<IEnumerable<TResult>> RequestCollection<TQuery, TResult>(IQuery<IEnumerable<TResult>> query)
            where TQuery : IQuery<IEnumerable<TResult>>
        {
            return Request<TQuery, IEnumerable<TResult>>(query);
        }

        public Task<IEnumerable<TResult>> RequestCollection<TQuery, TResult>()
            where TQuery : IQuery<IEnumerable<TResult>>, new()
        {
            return RequestCollection<TQuery, TResult>(new TQuery());
        }
    }
}

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

        public async Task<TResult> Execute<TResult>(ICommand<TResult> command)
        {
            var commandName = command.GetType().Name;
            try
            {
                _logger.LogInformation($"Executing {commandName} command.");
                var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
                var handler = _context.Resolve(handlerType);
                if (handler == null)
                {
                    throw new NotSupportedException($"No handler registered for command '{commandName}' with return type of '{typeof(TResult)}'");
                }

                // Hurray! To the C# type infrance system that is so stupid it can't figure out that GenerateReportCommand is in fact ICommand<TResult>
                var result = await (Task<TResult>)handler
                    .GetType()
                    .GetMethod("Handle")
                    .Invoke(handler, new[] { command });

                _logger.LogInformation($"Executed {commandName} command.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {commandName} command.");
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

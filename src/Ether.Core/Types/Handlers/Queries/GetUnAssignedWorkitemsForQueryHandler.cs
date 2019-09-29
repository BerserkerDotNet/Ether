using System;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetUnAssignedWorkitemsForQueryHandler : IQueryHandler<GetUnAssignedWorkitemsForQuery, UnAssignedWorkitemsViewModel>
    {
        private readonly IIndex<string, IDataSource> _dataSources;
        private readonly IRepository _repository;
        private readonly ILogger<GetActiveWorkitemsForProfileHandler> _logger;

        public GetUnAssignedWorkitemsForQueryHandler(IIndex<string, IDataSource> dataSources, IRepository repository, ILogger<GetActiveWorkitemsForProfileHandler> logger)
        {
            _dataSources = dataSources;
            _repository = repository;
            _logger = logger;
        }

        public async Task<UnAssignedWorkitemsViewModel> Handle(GetUnAssignedWorkitemsForQuery query)
        {
            if (!_dataSources.TryGetValue(query.DataSourceType, out var dataSource))
            {
                throw new ArgumentException($"Data source of type {query.DataSourceType} is not supported.");
            }

            return await dataSource.GetUnAssignedFromQuery(query.QueryId);
        }
    }
}

using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels.Types;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetJobDetailsByIdHandler : IQueryHandler<GetJobDetailsById, JobDetails>
    {
        private readonly IRepository _repository;

        public GetJobDetailsByIdHandler(IRepository repository)
        {
            this._repository = repository;
        }

        public async Task<JobDetails> Handle(GetJobDetailsById input)
        {
            if (input.Id == Guid.Empty)
            {
                return null;
            }

            var result = await _repository.GetSingleAsync<JobLog>(input.Id);
            return result.Details;
        }
    }
}

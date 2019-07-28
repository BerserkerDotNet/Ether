using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels.Types;

namespace Ether.Core.Types.Queries
{
    public class GetJobDetailsById : IQuery<JobDetails>
    {
        public GetJobDetailsById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}

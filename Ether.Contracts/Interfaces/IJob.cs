using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.ViewModels.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IJob
    {
        Task<JobDetails> Execute(IReadOnlyDictionary<string, object> parameters);
    }
}

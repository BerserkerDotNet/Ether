using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces
{
    public interface IJobRunner
    {
        Task RunJob<T>(Dictionary<string, object> parameters)
            where T : IJob;
    }
}

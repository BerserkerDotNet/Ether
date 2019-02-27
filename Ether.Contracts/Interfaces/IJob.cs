using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces
{
    public interface IJob
    {
        Task Execute(IReadOnlyDictionary<string, object> parameters);
    }
}

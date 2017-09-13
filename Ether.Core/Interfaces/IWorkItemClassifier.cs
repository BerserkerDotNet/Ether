using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Core.Interfaces
{
    public interface IWorkItemClassifier
    {
        Task<string> Classify(int id);
        Task<string> ClassifyAll(IEnumerable<int> ids);
    }
}

using System.Threading.Tasks;

namespace Ether.Core.Interfaces
{
    public interface IProgressReporter
    {
        Task Reset();
        Task Report(string message, float moveProgressBy = 0);
    }
}

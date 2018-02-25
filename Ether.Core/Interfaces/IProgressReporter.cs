using System.Threading.Tasks;

namespace Ether.Core.Interfaces
{
    public interface IProgressReporter
    {
        Task Report(string message, float moveProgressBy = 0);
    }
}

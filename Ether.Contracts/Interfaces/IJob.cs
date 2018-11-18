using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces
{
    public interface IJob
    {
        Task Execute();
    }
}

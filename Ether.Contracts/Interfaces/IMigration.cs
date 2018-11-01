using System.Threading.Tasks;

namespace Ether.Contracts.Interfaces
{
    public interface IMigration
    {
        Task Run();
    }
}

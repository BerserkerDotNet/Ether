using System.Threading.Tasks;
using VSTS.Net.Interfaces;

namespace Ether.Vsts.Interfaces
{
    public interface IVstsClientFactory
    {
        Task<IVstsClient> GetClient(string token = null);

        Task<IVstsPullRequestsClient> GetPullRequestsClient(string token = null);

        Task<IVstsIdentityClient> GetIdentityClient(string token = null);
    }
}
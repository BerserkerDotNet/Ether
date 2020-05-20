using System;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;

namespace Ether.Vsts.Interfaces
{
    public interface IVstsClientFactory
    {
        Task<IVstsClient> GetClient(Guid organizationId, string token = null);

        Task<IVstsPullRequestsClient> GetPullRequestsClient(Guid organizationId, string token = null);

        Task<IVstsIdentityClient> GetIdentityClient(string token = null);

        Task<IVstsWorkItemsClient> GetWorkItemsClient(Guid? identityId);
    }
}
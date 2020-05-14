using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteOrganization : IAsyncAction<OrganizationViewModel>
    {
        private readonly EtherClient _client;

        public DeleteOrganization(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, OrganizationViewModel organization)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                await _client.Delete<OrganizationViewModel>(organization.Id);
            });

            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchOrganizations>();
        }
    }
}

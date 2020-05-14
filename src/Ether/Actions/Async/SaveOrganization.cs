using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveOrganization : IAsyncAction<OrganizationViewModel>
    {
        private readonly EtherClient _client;

        public SaveOrganization(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, OrganizationViewModel organization)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                await _client.Save(organization);

                // TODO: instead of refresh insert?
                await dispatcher.Dispatch<FetchOrganizations>();
            });
        }
    }
}

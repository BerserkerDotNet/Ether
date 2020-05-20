using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchOrganizations : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchOrganizations(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                var organizations = await _client.GetAll<OrganizationViewModel>();

                dispatcher.Dispatch(new ReceiveOrganizationsAction
                {
                    Organizations = organizations
                });
            });
        }
    }
}

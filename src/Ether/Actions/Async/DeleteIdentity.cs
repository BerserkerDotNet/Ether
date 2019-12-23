using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteIdentity : IAsyncAction<IdentityViewModel>
    {
        private readonly EtherClient _client;

        public DeleteIdentity(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, IdentityViewModel identity)
        {
            await _client.Delete<IdentityViewModel>(identity.Id);

            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchIdentities>();
        }
    }
}

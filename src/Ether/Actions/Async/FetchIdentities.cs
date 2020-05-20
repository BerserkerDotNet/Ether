using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchIdentities : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchIdentities(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                var identities = await _client.GetAll<IdentityViewModel>();

                dispatcher.Dispatch(new ReceiveIdentitiesAction
                {
                    Identities = identities
                });
            });
        }
    }
}

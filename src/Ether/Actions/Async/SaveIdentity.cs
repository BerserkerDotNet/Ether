using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveIdentity : IAsyncAction<IdentityViewModel>
    {
        private readonly EtherClient _client;

        public SaveIdentity(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, IdentityViewModel identity)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                await _client.Save(identity);

                // TODO: instead of refresh insert?
                await dispatcher.Dispatch<FetchIdentities>();
            });
        }
    }
}

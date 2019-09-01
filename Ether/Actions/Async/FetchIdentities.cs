using System.Threading.Tasks;
using Ether.Redux.Interfaces;
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
            var identities = await _client.GetAll<IdentityViewModel>();
            dispatcher.Dispatch(new ReceiveIdentitiesAction
            {
                Identities = identities
            });
        }
    }
}

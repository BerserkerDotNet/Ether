using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchProfiles : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchProfiles(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            var profiles = await _client.GetAll<ProfileViewModel>();
            dispatcher.Dispatch(new ReceiveProfilesAction
            {
                Profiles = profiles
            });
        }
    }
}

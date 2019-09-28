using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveProfile : IAsyncAction<ProfileViewModel>
    {
        private readonly EtherClient _client;

        public SaveProfile(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, ProfileViewModel profile)
        {
            await _client.Save(profile);
            // TODO: instead of refresh insert?
            await dispatcher.Dispatch<FetchProfiles>();
        }
    }
}

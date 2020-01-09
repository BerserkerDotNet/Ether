using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class DeleteProfile : IAsyncAction<ProfileViewModel>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public DeleteProfile(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, ProfileViewModel profile)
        {
            await _client.Delete<ProfileViewModel>(profile.Id);

            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchProfiles>();
            _toaster.Add($"Profile {profile.Name} was deleted successfully.", MatToastType.Success, "Delete", MatIconNames.Delete);
        }
    }
}

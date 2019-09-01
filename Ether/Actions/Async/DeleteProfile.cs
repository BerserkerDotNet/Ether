using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteProfile : IAsyncAction<ProfileViewModel>
    {
        private readonly EtherClient _client;
        private readonly JsUtils _jsUtils;

        public DeleteProfile(EtherClient client, JsUtils jsUtils)
        {
            _client = client;
            _jsUtils = jsUtils;
        }

        public async Task Execute(IDispatcher dispatcher, ProfileViewModel profile)
        {
            await _client.Delete<ProfileViewModel>(profile.Id);
            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchProfiles>();
            await _jsUtils.NotifySuccess("Delete", $"Profile {profile.Name} was deleted successfully.");
        }
    }
}

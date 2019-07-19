using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ProfilesStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public ProfilesStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<ProfileViewModel> Profiles => _state.Profiles;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Profiles = null;
            }

            if (_state.Profiles == null)
            {
                _state.Profiles = await _client.GetAll<ProfileViewModel>();
            }
        }

        public async Task UpdateAsync(ProfileViewModel profile)
        {
            await _client.Save(profile);
            if (!Profiles.Any(p => p.Id == profile.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteAsync(ProfileViewModel profile)
        {
            await _client.Delete<ProfileViewModel>(profile.Id);
            await LoadAsync(hard: true);
        }

        public async Task FetchWorkItems(IEnumerable<Guid> members)
        {
            await _client.RunWorkitemsJob(members, isReset: false);
        }

        public async Task ResetWorkItems(IEnumerable<Guid> members)
        {
            await _client.RunWorkitemsJob(members, isReset: true);
        }
    }
}

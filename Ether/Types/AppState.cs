using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types
{
    public class AppState
    {
        public IReadOnlyList<VstsProjectViewModel> Projects { get; set; }

        public IEnumerable<TeamMemberViewModel> TeamMembers { get; set; }
    }

    public class TeamMembersStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public TeamMembersStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<TeamMemberViewModel> Members => _state.TeamMembers;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.TeamMembers = null;
            }

            if (_state.TeamMembers == null)
            {
                _state.TeamMembers = await _client.GetAll<TeamMemberViewModel>();
            }
        }

        public async Task UpdateMemberAsync(TeamMemberViewModel member)
        {
            await _client.Save(member);
            if (!Members.Any(m => m.Id == member.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteMemberAsync(TeamMemberViewModel member)
        {
            await _client.Delete<TeamMemberViewModel>(member.Id);
            await LoadAsync(hard: true);
        }

        private void InvalidateState()
        {
            _state.TeamMembers = null;
        }
    }
}

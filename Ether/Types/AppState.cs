using System.Collections.Generic;
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

        public async ValueTask<IEnumerable<TeamMemberViewModel>> GetAsync()
        {
            if (_state.TeamMembers == null)
            {
                _state.TeamMembers = await _client.GetAll<TeamMemberViewModel>();
            }

            return _state.TeamMembers;
        }

        public async Task UpdateMember(TeamMemberViewModel member)
        {
            await _client.Save(member);
            InvalidateState();
        }

        public async Task DeleteMember(TeamMemberViewModel member)
        {
            await _client.Delete<TeamMemberViewModel>(member.Id);
            InvalidateState();
        }


        private void InvalidateState()
        {
            _state.TeamMembers = null;
        }
    }
}

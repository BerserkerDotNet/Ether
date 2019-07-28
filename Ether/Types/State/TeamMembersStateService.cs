using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
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

        public async Task UpdateAsync(TeamMemberViewModel member)
        {
            await _client.Save(member);
            if (!Members.Any(m => m.Id == member.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteAsync(TeamMemberViewModel member)
        {
            await _client.Delete<TeamMemberViewModel>(member.Id);
            await LoadAsync(hard: true);
        }

        public async Task FetchWorkItems(TeamMemberViewModel member)
        {
            await _client.RunWorkitemsJob(new[] { member.Id }, isReset: false);
        }

        public async Task ResetWorkItems(TeamMemberViewModel member)
        {
            await _client.RunWorkitemsJob(new[] { member.Id }, isReset: true);
        }

        public async Task FetchPullRequests(TeamMemberViewModel member)
        {
            await _client.RunPullRequestsJob(new[] { member.Id }, isReset: false);
        }

        public async Task ResetPullRequests(TeamMemberViewModel member)
        {
            await _client.RunPullRequestsJob(new[] { member.Id }, isReset: true);
        }
    }
}

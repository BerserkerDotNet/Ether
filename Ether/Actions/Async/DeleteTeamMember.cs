using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteTeamMember : IAsyncAction<TeamMemberViewModel>
    {
        private readonly EtherClient _client;

        public DeleteTeamMember(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, TeamMemberViewModel member)
        {
            await _client.Delete<TeamMemberViewModel>(member.Id);
            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchMembers>();
        }
    }
}

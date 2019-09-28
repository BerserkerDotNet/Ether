using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveTeamMember : IAsyncAction<TeamMemberViewModel>
    {
        private readonly EtherClient _client;

        public SaveTeamMember(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, TeamMemberViewModel member)
        {
            await _client.Save(member);
            await dispatcher.Dispatch<FetchMembers>();
        }
    }
}

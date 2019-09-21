using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchMembers : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchMembers(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            var members = await _client.GetAll<TeamMemberViewModel>();
            dispatcher.Dispatch(new ReceiveMembersAction
            {
                Members = members
            });
        }
    }
}

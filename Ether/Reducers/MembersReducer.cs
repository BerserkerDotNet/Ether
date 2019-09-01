using Ether.Actions;
using Ether.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class MembersReducer : IReducer<TeamMembersState>
    {
        public TeamMembersState Reduce(TeamMembersState state, IAction action)
        {
            switch (action)
            {
                case ReceiveMembersAction a:
                    return new TeamMembersState(a.Members);
                default:
                    return state;
            }
        }
    }
}

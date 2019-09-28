using Ether.Actions;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class ReportsReducer : IReducer<ReportsState>
    {
        public ReportsState Reduce(ReportsState state, IAction action)
        {
            switch (action)
            {
                case ReceiveReportsAction a:
                    return new ReportsState(a.Reports);
                default:
                    return state;
            }
        }
    }
}

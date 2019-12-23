using BlazorState.Redux.Interfaces;
using Ether.Actions;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class RepositoriesReducer : IReducer<RepositoriesState>
    {
        public RepositoriesState Reduce(RepositoriesState state, IAction action)
        {
            switch (action)
            {
                case ReceiveRepositoriesAction a:
                    return new RepositoriesState(a.Repositories);
                default:
                    return state;
            }
        }
    }
}

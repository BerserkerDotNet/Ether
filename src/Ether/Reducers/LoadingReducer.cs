using BlazorState.Redux.Interfaces;
using Ether.Actions;

namespace Ether.Reducers
{
    public class LoadingReducer : IReducer<int>
    {
        public int Reduce(int state, IAction action)
        {
            switch (action)
            {
                case Loading a:
                    return a.IsDone ? state - 1 : state + 1;
                default:
                    return state;
            }
        }
    }
}

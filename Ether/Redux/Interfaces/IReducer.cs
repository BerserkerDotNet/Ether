using System;

namespace Ether.Redux.Interfaces
{
    public interface IReducer<TState>
    {
        TState Reduce(TState state, IAction action);
    }
}

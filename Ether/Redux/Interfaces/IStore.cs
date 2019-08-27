using System;

namespace Ether.Redux.Interfaces
{
    public interface IStore<TState>
    {
        event EventHandler<EventArgs> OnStateChanged;

        TState State { get; }

        void Dispatch(IAction action);
    }
}

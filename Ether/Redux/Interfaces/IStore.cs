using System;

namespace Ether.Redux.Interfaces
{
    public interface IStore<TState> : IDispatcher
    {
        event EventHandler<EventArgs> OnStateChanged;

        TState State { get; }
    }
}

using System;

namespace Ether.Redux.Interfaces
{
    public interface INavigationTracker<TState> : IDisposable
    {
        void Start(IDispatcher dispatcher);

        void Navigate(TState state);
    }
}

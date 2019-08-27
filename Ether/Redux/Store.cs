using System;
using Ether.Redux.Interfaces;

namespace Ether.Redux
{
    public class Store<TState> : IStore<TState>
    {
        private readonly IReducer<TState> _rootReducer;

        public Store(IReducer<TState> rootReducer)
        {
            _rootReducer = rootReducer;
        }

        public event EventHandler<EventArgs> OnStateChanged;

        public TState State { get; private set; }

        public void Dispatch(IAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            State = _rootReducer.Reduce(State, action);
            OnStateChanged?.Invoke(this, EventArgs.Empty); // TODO: concrete type for event handler
        }
    }

}

using System;
using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;

namespace Ether.Redux
{
    public class Store<TState> : IStore<TState>
    {
        private readonly IReducer<TState> _rootReducer;
        private readonly IActionResolver _actionResolver;
        private readonly LocalStorage _localStorage;

        public Store(IReducer<TState> rootReducer, IActionResolver actionResolver, LocalStorage localStorage)
        {
            _rootReducer = rootReducer;
            _actionResolver = actionResolver;
            _localStorage = localStorage;
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
            _localStorage.SetItem("State", State);
            Console.WriteLine($"[Redux Store] - Executed action {action.GetType().Name}");
            OnStateChanged?.Invoke(this, EventArgs.Empty); // TODO: concrete type for event handler
        }

        public async Task Dispatch<TAsyncAction, TProperty>(TProperty property)
            where TAsyncAction : IAsyncAction<TProperty>
        {
            var action = _actionResolver.Resolve<TAsyncAction>();
            await action.Execute(this, property);
        }
    }
}

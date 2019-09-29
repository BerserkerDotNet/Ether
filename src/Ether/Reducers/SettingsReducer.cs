using Ether.Actions;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class SettingsReducer : IReducer<SettingsState>
    {
        public SettingsState Reduce(SettingsState state, IAction action)
        {
            switch (action)
            {
                case ReceiveIdentitiesAction a:
                    return new SettingsState(a.Identities, state?.DataSource);
                case ReceiveDataSourceConfig a:
                    return new SettingsState(state?.Identities, a.Config);
                default:
                    return state;
            }
        }
    }
}

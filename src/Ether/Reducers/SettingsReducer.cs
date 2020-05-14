using BlazorState.Redux.Interfaces;
using Ether.Actions;
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
                    return new SettingsState(a.Identities, state?.Organizations);
                case ReceiveOrganizationsAction a:
                    return new SettingsState(state?.Identities, a.Organizations);
                default:
                    return state;
            }
        }
    }
}

using Ether.Actions;
using Ether.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class ProfilesReducer : IReducer<ProfilesState>
    {
        public ProfilesState Reduce(ProfilesState state, IAction action)
        {
            switch (action)
            {
                case ReceiveProfilesAction a:
                    return new ProfilesState(a.Profiles);
                default:
                    return state;
            }
        }
    }
}

using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ProfilesState
    {
        public ProfilesState(IEnumerable<ProfileViewModel> profiles)
        {
            Profiles = profiles;
        }

        public IEnumerable<ProfileViewModel> Profiles { get; private set; }
    }
}

using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class ProfilesState
    {
        [JsonConstructor]
        public ProfilesState(IEnumerable<ProfileViewModel> profiles)
        {
            Profiles = profiles;
        }

        public IEnumerable<ProfileViewModel> Profiles { get; private set; }
    }
}

using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class SettingsState
    {
        [JsonConstructor]
        public SettingsState(IEnumerable<IdentityViewModel> identities, IEnumerable<OrganizationViewModel> organizations)
        {
            Identities = identities;
            Organizations = organizations;
        }

        public IEnumerable<IdentityViewModel> Identities { get; private set; }

        public IEnumerable<OrganizationViewModel> Organizations { get; private set; }
    }
}

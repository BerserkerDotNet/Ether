using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class SettingsState
    {
        [JsonConstructor]
        public SettingsState(IEnumerable<IdentityViewModel> identities, VstsDataSourceViewModel dataSource)
        {
            Identities = identities;
            DataSource = dataSource;
        }

        public IEnumerable<IdentityViewModel> Identities { get; private set; }

        public VstsDataSourceViewModel DataSource { get; private set; }
    }
}

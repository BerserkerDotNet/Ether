using Ether.ViewModels;
using System.Collections.Generic;

namespace Ether.Types.State
{
    public class SettingsState
    {
        public SettingsState(IEnumerable<IdentityViewModel> identities, VstsDataSourceViewModel dataSource)
        {
            Identities = identities;
            DataSource = dataSource;
        }

        public IEnumerable<IdentityViewModel> Identities { get; private set; }

        public VstsDataSourceViewModel DataSource { get; private set; }
    }
}

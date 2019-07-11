using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class DashboardSettingsViewModel : ViewModelWithId
    {
        public Guid ProfileId { get; set; }

        public IEnumerable<FilterSubTeam> SubTeams { get; set; }
    }

    public class FilterSubTeam : ViewModelWithId
    {
        public string Name { get; set; }

        public IEnumerable<string> Members { get; set; }
    }
}

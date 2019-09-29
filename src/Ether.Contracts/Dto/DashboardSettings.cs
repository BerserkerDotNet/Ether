using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Contracts.Dto
{
    public class DashboardSettings : BaseDto
    {
        public string DashboardName { get; set; }

        public Guid ProfileId { get; set; }

        public Guid? WorkitemsQueryId { get; set; }

        public IEnumerable<FilterSubTeam> SubTeams { get; set; }
    }
}

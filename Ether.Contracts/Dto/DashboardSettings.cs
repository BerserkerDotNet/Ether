using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Contracts.Dto
{
    public class DashboardSettings : BaseDto
    {
        public Guid ProfileId { get; set; }

        public IEnumerable<FilterSubTeam> SubTeams { get; set; }
    }
}

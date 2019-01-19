using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public class ClassificationScope
    {
        public ClassificationScope(IEnumerable<TeamMemberViewModel> team, DateTime startDate, DateTime endDate)
        {
            Team = team;
            StartDate = startDate;
            EndDate = endDate;
        }

        public IEnumerable<TeamMemberViewModel> Team { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }
    }
}

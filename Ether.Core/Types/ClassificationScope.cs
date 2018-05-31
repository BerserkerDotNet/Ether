using System;
using System.Collections.Generic;
using Ether.Core.Models.DTO;

namespace Ether.Core.Types
{
    public class ClassificationScope
    {
        public ClassificationScope(IEnumerable<TeamMember> team, DateTime startDate, DateTime endDate)
        {
            Team = team;
            StartDate = startDate;
            EndDate = endDate;
        }

        public IEnumerable<TeamMember> Team { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

    }
}
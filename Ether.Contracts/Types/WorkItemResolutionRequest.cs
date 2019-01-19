using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Contracts.Types
{
    public class WorkItemResolutionRequest
    {
        public WorkItemViewModel WorkItem { get; set; }

        public IEnumerable<TeamMemberViewModel> Team { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}

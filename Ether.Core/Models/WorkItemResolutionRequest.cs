using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using System;
using System.Collections.Generic;

namespace Ether.Core.Models
{
    public class WorkItemResolutionRequest
    {
        public VSTSWorkItem WorkItem { get; set; }
        public IEnumerable<TeamMember> Team { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

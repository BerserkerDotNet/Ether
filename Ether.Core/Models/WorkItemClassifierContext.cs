using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using System.Collections.Generic;

namespace Ether.Core.Models
{
    public class WorkItemResolutionRequest
    {
        public int WorkItemId { get; set; }
        public string WorkItemType { get; set; }

        public IEnumerable<WorkItemUpdate> WorkItemUpdates { get; set; }
        public IEnumerable<TeamMember> Team { get; set; }
    }
}

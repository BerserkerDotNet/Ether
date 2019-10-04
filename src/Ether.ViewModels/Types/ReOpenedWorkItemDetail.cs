using System;

namespace Ether.ViewModels.Types
{
    public class ReOpenedWorkItemDetail
    {
        public int WorkItemId { get; set; }

        public string WorkItemTitle { get; set; }

        public string WorkItemProject { get; set; }

        public string WorkItemType { get; set; }

        public DateTime ReOpenedDate { get; set; }

        public UserReference AssociatedUser { get; set; }
    }
}

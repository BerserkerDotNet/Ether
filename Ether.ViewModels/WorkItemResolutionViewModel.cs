using System;

namespace Ether.ViewModels
{
    public class WorkItemResolutionViewModel
    {
        public int WorkItemId { get; private set; }

        public string WorkItemTitle { get; private set; }

        public string WorkItemType { get; private set; }

        public string Resolution { get; private set; }

        public string Reason { get; private set; }

        public DateTime ResolutionDate { get; private set; }

        public string MemberEmail { get; private set; }

        public string MemberName { get; private set; }
    }
}

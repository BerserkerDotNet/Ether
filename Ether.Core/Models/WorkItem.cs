using System.Diagnostics;

namespace Ether.Core.Models
{
    [DebuggerDisplay("{Type} - {Id} {Resolution.Resolution}/{Resolution.Reason}")]
    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }

        public WorkItemResolution Resolution { get; set; }
    }
}

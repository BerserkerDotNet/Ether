namespace Ether.ViewModels.Types
{
    public class WorkItemDetail
    {
        public int WorkItemId { get; set; }

        public string WorkItemTitle { get; set; }

        public string WorkItemType { get; set; }

        public string WorkItemProject { get; set; }

        public string Tags { get; set; }

        public float OriginalEstimate { get; set; }

        public float EstimatedToComplete { get; set; }

        public float TimeSpent { get; set; }

        public string Reason { get; set; }
    }
}

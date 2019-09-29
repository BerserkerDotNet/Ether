namespace Ether.Api.Types
{
    public class JobsConfiguration
    {
        public int PullRequestJobRecurrence { get; set; }

        public int WorkItemsJobRecurrence { get; set; }

        public int RetentionJobRecurrence { get; set; }
    }
}

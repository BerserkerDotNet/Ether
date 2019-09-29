using System;

namespace Ether.ViewModels
{
    public class WorkitemPullRequest
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public TimeSpan TimeActive { get; set; }

        public string State { get; set; }

        public int NumberOfApprovals { get; set; }
    }
}

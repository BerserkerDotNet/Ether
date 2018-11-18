using System;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class VstsPullRequestViewModel
    {
        public int PullRequestId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public VstsPullRequestState State { get; set; }

        public int Comments { get; set; }

        public int Iterations { get; set; }

        public Guid Repository { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Completed { get; set; }

        public DateTime? LastSync { get; set; }
    }
}

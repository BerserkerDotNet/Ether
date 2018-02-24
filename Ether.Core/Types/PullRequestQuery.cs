using Ether.Core.Models.VSTS;
using System;
using System.Collections.Generic;

namespace Ether.Core.Types
{
    public class PullRequestQuery
    {
        public PullRequestQuery(DateTime fromDate)
        {
            Parameters = new Dictionary<string, string>();
            Filter = p => true;
            FromDate = fromDate;
        }

        public PullRequestQuery WithParameter(string name, string value)
        {
            Parameters.Add(name, value);
            return this;
        }

        public PullRequestQuery WithFilter(Func<PullRequest, bool> filter)
        {
            Filter = filter;
            return this;
        }

        public Dictionary<string, string> Parameters { get; set; }
        public Func<PullRequest, bool> Filter { get; set; }
        public DateTime FromDate { get; }

        public static PullRequestQuery New(DateTime fromDate)
        {
            return new PullRequestQuery(fromDate);
        }
    }
}

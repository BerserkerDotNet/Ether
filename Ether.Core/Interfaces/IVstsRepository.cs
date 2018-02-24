using Ether.Core.Models.VSTS;
using System;
using System.Collections.Generic;

namespace Ether.Core.Interfaces
{
    public interface IVstsRepository
    {
        IEnumerable<PullRequest> GetPullRequests(string project, string repository, PullRequestQuery query);
    }

    public class PullRequestQuery
    {
        public PullRequestQuery(DateTime startDate, DateTime endDate)
        {
            EndDate = endDate;
            StartDate = startDate;
            Parameters = new Dictionary<string, string>();
        }

        public PullRequestQuery WithParameter(string name, string value)
        {
            Parameters.Add(name, value);
            return this;
        }

        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Dictionary<string, string> Parameters { get; set; }
        public Func<PullRequest, bool> Filter { get; set; }

        public static PullRequestQuery New(DateTime startDate, DateTime endDate)
        {
            return new PullRequestQuery(startDate, endDate);
        }
    }
}

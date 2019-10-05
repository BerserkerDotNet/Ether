using System;
using System.Collections.Generic;
using System.Linq;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class ReOpenedWorkItemsReportViewModel : ReportViewModel
    {
        public ReOpenedWorkItemsReportViewModel()
        {
            Details = Enumerable.Empty<ReOpenedWorkItemDetail>();
        }

        public IEnumerable<ReOpenedWorkItemDetail> Details { get; set; }

        public IReadOnlyDictionary<string, int> ResolvedWorkItemsLookup { get; set; }

        public IReadOnlyDictionary<string, string> MembersLookup { get; set; }

        public int TotalReopened => Details.Count();

        public int TotalResolved => ResolvedWorkItemsLookup.Values.Sum();

        public double TotalPercentage => Math.Round(((double)TotalReopened / TotalResolved) * 100, 2);

        public int GetRepopenedCount(string email) => Details
            .Where(r => r.AssociatedUser.Email == email)
            .Count();

        public int GetResolvedCount(string email) => ResolvedWorkItemsLookup.ContainsKey(email) ? ResolvedWorkItemsLookup[email] : 0;

        public double GetPercentage(string email)
        {
            if (!ResolvedWorkItemsLookup.ContainsKey(email))
            {
                return 0;
            }

            var reopenedCount = GetRepopenedCount(email);
            var resolved = ResolvedWorkItemsLookup[email];

            return Math.Round(((double)reopenedCount / resolved) * 100, 2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Dashboard
{
    public class ActiveWorkitemDetailsBase : ComponentBase
    {
        [Parameter]
        public WorkitemInformationViewModel Workitem { get; set; }

        protected string FormatActiveTime(TimeSpan activeTime)
        {
            if (activeTime.TotalDays < 1)
            {
                return "1 day";
            }
            else
            {
                return $"{Math.Round(activeTime.TotalDays, 0).ToString()} days";
            }
        }

        protected IEnumerable<WorkitemPullRequest> FilterRelevantPullRequests(IEnumerable<WorkitemPullRequest> prs) => prs.Where(p => !string.IsNullOrEmpty(p.Author));
    }
}

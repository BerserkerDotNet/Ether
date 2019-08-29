using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Settings
{
    public class JobLogsProps
    {
        public IEnumerable<JobLogViewModel> Items { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public Func<JobLogViewModel, Task<ViewModels.Types.JobDetails>> OnDetailsRequested { get; set; }

        public EventCallback OnRefresh { get; set; }

        public EventCallback OnNextPage { get; set; }

        public EventCallback OnPreviousPage { get; set; }
    }
}

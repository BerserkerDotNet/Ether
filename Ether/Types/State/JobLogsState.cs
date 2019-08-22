using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class JobLogsState
    {
        public JobLogsState(IEnumerable<JobLogViewModel> items, int currentPage, int totalPages)
        {
            Items = items;
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }

        public IEnumerable<JobLogViewModel> Items { get; private set; }

        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }
    }
}

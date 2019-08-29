using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceivedJobLogsPage : IAction
    {
        public IEnumerable<JobLogViewModel> Logs { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
    }
}

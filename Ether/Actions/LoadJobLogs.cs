using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class LoadJobLogs : IAction
    {
        public IEnumerable<JobLogViewModel> Logs { get; set; }

        public int TotalPages { get; set; }
    }
}

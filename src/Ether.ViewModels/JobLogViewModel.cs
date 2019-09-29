using System;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class JobLogViewModel : ViewModelWithId
    {
        public string JobType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public JobExecutionResult Result { get; set; }

        public string Error { get; set; }

        public JobDetails Details { get; set; }
    }
}

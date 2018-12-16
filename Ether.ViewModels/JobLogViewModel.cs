using System;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class JobLogViewModel : ViewModelWithId
    {
        public string JobType { get; set; }

        public DateTime CompletedOn { get; set; }

        public JobExecutionResult Result { get; set; }

        public string Message { get; set; }

        public TimeSpan ExecutionTime { get; set; }
    }
}

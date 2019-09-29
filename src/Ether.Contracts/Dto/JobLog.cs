using System;
using Ether.Contracts.Types;
using Ether.ViewModels.Types;

namespace Ether.Contracts.Dto
{
    public class JobLog : BaseDto
    {
        public string JobType { get; set; }

        public JobExecutionState Result { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Error { get; set; }

        public JobDetails Details { get; set; }
    }
}

using System;
using Ether.Contracts.Types;

namespace Ether.Contracts.Dto
{
    public class JobLog : BaseDto
    {
        public string JobType { get; set; }

        public DateTime CompletedOn { get; set; }

        public JobExecutionResult Result { get; set; }

        public string Message { get; set; }

        public TimeSpan ExecutionTime { get; set; }
    }
}

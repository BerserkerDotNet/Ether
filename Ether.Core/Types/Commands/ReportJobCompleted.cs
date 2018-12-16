using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;

namespace Ether.Core.Types.Commands
{
    public class ReportJobCompleted : ICommand
    {
        public ReportJobCompleted(string jobType, JobExecutionResult result, string message, TimeSpan executionTime)
        {
            JobType = jobType;
            CompletedOn = DateTime.UtcNow;
            ExecutionTime = executionTime;
            Result = result;
            Message = message;
        }

        public string JobType { get; set; }

        public JobExecutionResult Result { get; set; }

        public string Message { get; set; }

        public DateTime CompletedOn { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public static ReportJobCompleted GetSuccessful(string jobType, TimeSpan executionTime)
            => new ReportJobCompleted(jobType, JobExecutionResult.Successful, string.Empty, executionTime);

        public static ReportJobCompleted GetFailed(string jobType, string message, TimeSpan executionTime)
            => new ReportJobCompleted(jobType, JobExecutionResult.Failed, message, executionTime);
    }
}

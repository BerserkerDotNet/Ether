using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;

namespace Ether.Core.Types.Commands
{
    public class ReportJobState : ICommand
    {
        public ReportJobState(Guid jobId, string jobType, JobExecutionState result, string message, TimeSpan executionTime)
        {
            JobId = jobId;
            JobType = jobType;
            CompletedOn = DateTime.UtcNow;
            ExecutionTime = executionTime;
            Result = result;
            Message = message;
        }

        public Guid JobId { get; set; }

        public string JobType { get; set; }

        public JobExecutionState Result { get; set; }

        public string Message { get; set; }

        public DateTime CompletedOn { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public static ReportJobState GetRunning(Guid jobId, string jobType)
            => new ReportJobState(jobId, jobType, JobExecutionState.InProgress, string.Empty, TimeSpan.Zero);

        public static ReportJobState GetSuccessful(Guid jobId, string jobType, TimeSpan executionTime)
            => new ReportJobState(jobId, jobType, JobExecutionState.Successful, string.Empty, executionTime);

        public static ReportJobState GetFailed(Guid jobId, string jobType, string message, TimeSpan executionTime)
            => new ReportJobState(jobId, jobType, JobExecutionState.Failed, message, executionTime);
    }
}

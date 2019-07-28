using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Ether.ViewModels.Types;

namespace Ether.Core.Types.Commands
{
    public class ReportJobState : ICommand
    {
        public ReportJobState(Guid jobId, string jobType, JobExecutionState result, string error, DateTime startTime, DateTime? endTime, JobDetails details = null)
        {
            JobId = jobId;
            JobType = jobType;
            Result = result;
            Error = error;
            StartTime = startTime;
            EndTime = endTime;
            Details = details;
        }

        public Guid JobId { get; set; }

        public string JobType { get; set; }

        public JobExecutionState Result { get; set; }

        public string Error { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public JobDetails Details { get; set; }

        public static ReportJobState GetRunning(Guid jobId, string jobType, DateTime startTime)
            => new ReportJobState(jobId, jobType, JobExecutionState.InProgress, string.Empty, startTime, endTime: null);

        public static ReportJobState GetSuccessful(Guid jobId, string jobType, DateTime startTime, DateTime endTime, JobDetails details)
            => new ReportJobState(jobId, jobType, JobExecutionState.Successful, string.Empty, startTime, endTime, details);

        public static ReportJobState GetFailed(Guid jobId, string jobType, string message, DateTime startTime, DateTime endTime)
            => new ReportJobState(jobId, jobType, JobExecutionState.Failed, message, startTime, endTime);

        public static ReportJobState GetAborted(Guid jobId, string jobType, DateTime startTime, DateTime endTime)
            => new ReportJobState(jobId, jobType, JobExecutionState.Failed, "Job aborted", startTime, endTime);
    }
}

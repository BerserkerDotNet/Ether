using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.ViewModels.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Jobs
{
    public class RetentionJob : IJob
    {
        private const int DefaultMaxAgeForReports = 30;
        private const int DefaultMaxAgeForJobLogs = 5;

        private readonly IRepository _repository;
        private readonly ILogger<RetentionJob> _logger;

        public RetentionJob(IRepository repository, ILogger<RetentionJob> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<JobDetails> Execute(IReadOnlyDictionary<string, object> parameters)
        {
            try
            {
                var oldReportsDate = DateTime.UtcNow.AddDays(-DefaultMaxAgeForReports);
                _logger.LogInformation("Deleting reports older than {ReportDate}", oldReportsDate);
                var reports = await _repository.GetAsync<ReportResult>(r => r.DateTaken <= oldReportsDate);
                _logger.LogInformation("Found {NumberOfReportsToDelete} reports to delete.", reports.Count());
                foreach (var report in reports)
                {
                    await _repository.DeleteAsync<ReportResult>(report.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reports");
            }

            try
            {
                var oldLogsDate = DateTime.UtcNow.AddDays(-DefaultMaxAgeForJobLogs);
                _logger.LogInformation("Deleting job logs older than {LogsDate}", oldLogsDate);
                var logs = await _repository.GetAsync<JobLog>(r => r.StartTime <= oldLogsDate);
                _logger.LogInformation("Found {NumberOfLogsToDelete} job logs  to delete.", logs.Count());
                foreach (var log in logs)
                {
                    await _repository.DeleteAsync<JobLog>(log.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logs");
            }

            return null;
        }
    }
}

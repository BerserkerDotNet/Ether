using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Ether.Jobs
{
    public class RetentionJob : IJob
    {
        private readonly IRepository _repository;
        private readonly ILogger<RetentionJob> _logger;

        public RetentionJob(IRepository repository, ILogger<RetentionJob> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void Execute()
        {
            var settings = _repository.GetSingleAsync<Settings>(_ => true)
                .GetAwaiter()
                .GetResult();
            if (settings == null)
            {
                _logger.LogWarning("Cannot perform retention, settings are not define.");
                return;
            }

            CleanWorkItems(settings);
            CleanReports(settings);

        }

        private void CleanWorkItems(Settings settings)
        {
            try
            {
                if (settings.WorkItemsSettings?.KeepLast == null)
                    return;

                var keepWorkitemsDate = DateTime.UtcNow.Subtract(settings.WorkItemsSettings.KeepLast.Value);
                var itemsToDelete = _repository.GetAll<VSTSWorkItem>()
                    .Where(w => w.CreatedDate < keepWorkitemsDate)
                    .Select(w => w.Id)
                    .ToList();
                var deletedWorkitemsCount = _repository.Delete<VSTSWorkItem>(w => itemsToDelete.Contains(w.Id));

                _logger.LogWarning("Deleted {NumberOfWorkitems} workitems that are older than '{KeepWorkitemsDate}'", deletedWorkitemsCount, keepWorkitemsDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning workitems.");
            }
        }

        private void CleanReports(Settings settings)
        {
            try
            {
                if (settings.ReportsSettings?.KeepLast == null)
                    return;

                var keepReportsDate = DateTime.UtcNow.Subtract(settings.ReportsSettings.KeepLast.Value);
                var deletedReportsCount = _repository.Delete<ReportResult>(w => w.DateTaken < keepReportsDate);
                _logger.LogWarning("Deleted {NumberOfReports} reports that are older than '{KeepReportsDate}'", deletedReportsCount, keepReportsDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning reports.");
            }
        }

        private void CleanPullRequests(Settings settings)
        {

        }
    }
}

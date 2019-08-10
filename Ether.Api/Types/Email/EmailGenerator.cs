using System.Linq;
using System.Threading.Tasks;
using Ether.EmailGenerator;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;
using static Ether.EmailGenerator.EmailGenerator;

namespace Ether.Api.Types.Email
{
    public class EmailGenerator
    {
        private readonly EmailGeneratorClient _client;
        private readonly ILogger<EmailGenerator> _logger;

        public EmailGenerator(EmailGeneratorClient client, ILogger<EmailGenerator> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Task<byte[]> Generate(WorkItemsReportViewModel report)
        {
            _logger.LogInformation("Requesting email for {ProfileId} - {ProfileName}");
            var reportExt = new Report();
            reportExt.Active.AddRange(report.ActiveWorkItems.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));
            reportExt.Completed.AddRange(report.ResolvedWorkItems.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));
            reportExt.Inreview.AddRange(report.WorkItemsInReview.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));

            var reply = _client.Generate(new Ether.EmailGenerator.EmailRequest
            {
                Id = report.Id.ToString(),
                Name = report.ProfileName,
                Report = reportExt,
                Template = new EmailTemplate { Subject = "Weekly status report - {Profile} - {Date}", Body = "dfssdf" }
            });

            return Task.FromResult(reply.File.ToByteArray());
        }
    }
}

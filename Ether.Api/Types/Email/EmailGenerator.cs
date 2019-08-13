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

        public Task<byte[]> Generate(ProfileViewModel profile, WorkItemsReportViewModel report, GenerateEmailViewModel model, VstsDataSourceViewModel vstsConfig)
        {
            _logger.LogInformation("Requesting email for {ProfileId} - {ProfileName}");
            var reportExt = new Report();
            reportExt.Active.AddRange(report.ActiveWorkItems.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Url = GetWorkItemUrl(vstsConfig, w.WorkItemProject, w.WorkItemId),
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));
            reportExt.Completed.AddRange(report.ResolvedWorkItems.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Url = GetWorkItemUrl(vstsConfig, w.WorkItemProject, w.WorkItemId),
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));
            reportExt.Inreview.AddRange(report.WorkItemsInReview.Select(w => new WorkItem()
            {
                Id = w.WorkItemId,
                Url = GetWorkItemUrl(vstsConfig, w.WorkItemProject, w.WorkItemId),
                Title = w.WorkItemTitle,
                Type = w.WorkItemType,
                Estimated = (int)w.EstimatedToComplete,
                Spent = (int)w.TimeSpent
            }));

            var request = new EmailRequest
            {
                Id = report.Id.ToString(),
                Name = report.ProfileName,
                Report = reportExt,
                Template = new EmailTemplate { Subject = profile.EmailSubject, Body = profile.EmailBody },
                Points = model.Points,
            };
            request.Attendance.AddRange(model.Attendance.Select(a =>
            {
                var attendance = new TeamAttendance
                {
                    Name = a.MemberName
                };
                attendance.Attendance.AddRange(a.Attendance.Select(ma => ma));
                return attendance;
            }));

            var reply = _client.Generate(request);

            return Task.FromResult(reply.File.ToByteArray());
        }

        private string GetWorkItemUrl(VstsDataSourceViewModel vstsConfig, string project, int workItemId)
        {
            return $"https://{vstsConfig.InstanceName}.visualstudio.com/{project}/_workitems/edit/{workItemId}";
        }
    }
}

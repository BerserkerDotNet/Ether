using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;

namespace Ether.EmailGenerator
{
    public class EmailGeneratorService : EmailGenerator.EmailGeneratorBase
    {
        private readonly ILogger<EmailGeneratorService> _logger;

        public EmailGeneratorService(ILogger<EmailGeneratorService> logger)
        {
            _logger = logger;
        }

        // TODO: Async?
        public override Task<EmailReply> Generate(EmailRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Generating report for {Id}", request.Id);

            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "EtherEmailGenerator");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                var filePath = Path.Combine(tempDir, $"{request.Id}.msg");

                var app = new Application();
                var mailItem = (MailItem)app.CreateItem(OlItemType.olMailItem);
                var subject = ApplyCommonPlaceholders(request.Template.Subject, request);
                var body = ApplyCommonPlaceholders(request.Template.Body, request);
                body = ApplyBodyPlaceholders(body, request);
                mailItem.Subject = subject;
                mailItem.HTMLBody = body;
                mailItem.SaveAs(filePath);

                var bytes = File.ReadAllBytes(filePath);

                return Task.FromResult(new EmailReply { File = ByteString.CopyFrom(bytes) });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while generating report.");
                throw;
            }
        }

        private string ApplyCommonPlaceholders(string value, EmailRequest request)
        {
            return value
                .Replace("{Profile}", request.Name)
                .Replace("{Date}", DateTime.Now.ToString("d"));
        }

        private string ApplyBodyPlaceholders(string value, EmailRequest request)
        {
            var styles = @"<style>
</style>";
            value = styles + value;

            var resolvedTable = CreateTable(request.Report.Completed, "#70AD47");
            var codeReviewTable = CreateTable(request.Report.Inreview, "#FFC000");
            var activeTable = CreateTable(request.Report.Active, "#5B9BD5");
            return value.Replace("{ResolvedItems}", resolvedTable)
                .Replace("{InReviewItems}", codeReviewTable)
                .Replace("{ActiveItems}", activeTable)
                .Replace("{ResolvedCount}", request.Report.Completed.Count.ToString())
                .Replace("{InReviewCount}", request.Report.Inreview.Count.ToString())
                .Replace("{ActiveCount}", request.Report.Active.Count.ToString())
                .Replace("{TeamCount}", "15");
        }

        private string CreateTable(IEnumerable<WorkItem> items, string color)
        {
            var table = new StringBuilder();
            table.Append($"<table style='border: 1px solid {color};border-collapse: collapse;'>");
            table.Append($"<thead style='background: {color};color: white;'>");
            table.Append("<tr>");
            table.Append($"<th style='border: 1px solid {color};border-right: 1px solid white;'>Id</th>");
            table.Append($"<th style='border: 1px solid {color};border-right: 1px solid white;'>Title</th>");
            table.Append($"<th style='border: 1px solid {color};border-right: 1px solid white;'>Type</th>");
            table.Append($"<th style='border: 1px solid {color};border-right: 1px solid white;'>Estimated (Days)</th>");
            table.Append($"<th style='border: 1px solid {color};border-right: 1px solid white;'>Time Spent (Days)</th>");
            table.Append("</tr>");
            table.Append("</thead>");
            table.Append("<tbody>");
            foreach (var item in items)
            {
                table.Append($"<tr>");
                table.Append($"<td style='border-top: solid {color} 1.0pt;border-left: none;border-bottom: solid {color} 1.0pt;border-right: none;'>{item.Id}</td>");
                table.Append($"<td style='border-top: solid {color} 1.0pt;border-left: none;border-bottom: solid {color} 1.0pt;border-right: none;'>{item.Title}</td>");
                table.Append($"<td style='border-top: solid {color} 1.0pt;border-left: none;border-bottom: solid {color} 1.0pt;border-right: none;'>{item.Type}</td>");
                table.Append($"<td style='border-top: solid {color} 1.0pt;border-left: none;border-bottom: solid {color} 1.0pt;border-right: none;'>{item.Estimated}</td>");
                table.Append($"<td style='border-top: solid {color} 1.0pt;border-left: none;border-bottom: solid {color} 1.0pt;border-right: none;'>{item.Spent}</td>");
                table.Append("</tr>");
            }

            table.Append("</tbody>");
            table.Append("</table>");

            return table.ToString();
        }
    }
}

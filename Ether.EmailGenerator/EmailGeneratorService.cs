using System;
using System.IO;
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
                var subject = request.Template.Subject
                    .Replace("{Profile}", request.Name)
                    .Replace("{Date}", DateTime.Now.ToString("d"));
                mailItem.Subject = subject;
                mailItem.HTMLBody = "<h1>Foo</h1>";
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
    }
}

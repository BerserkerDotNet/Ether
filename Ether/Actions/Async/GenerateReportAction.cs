using System;
using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Ether.Actions.Async
{
    public class GenerateReportAction : IAsyncAction<GenerateReportViewModel>
    {
        private readonly EtherClient _client;
        private readonly NavigationManager _navigation;
        private readonly ILogger<GenerateReportAction> _logger;

        public GenerateReportAction(EtherClient client, NavigationManager uriHelper, ILogger<GenerateReportAction> logger)
        {
            _client = client;
            _navigation = uriHelper;
            _logger = logger;
        }

        public async Task Execute(IDispatcher dispatcher, GenerateReportViewModel request)
        {
            try
            {
                dispatcher.Dispatch(new ReceiveReportRequestAction
                {
                    Request = request
                });

                var reportId = await _client.GenerateReport(request);
                _navigation.NavigateTo($"/reports/view/{request.ReportType}/{reportId}");

                // await JsUtils.NotifySuccess("Report", "Report generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");

                // await JsUtils.NotifyError("Report", $"Error generating report: {ex.Message}");
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Ether.Actions.Async
{
    public class GenerateReportAction : IAsyncAction<GenerateReportViewModel>
    {
        private readonly EtherClient _client;
        private readonly NavigationManager _navigation;
        private readonly IMatToaster _toaster;
        private readonly ILogger<GenerateReportAction> _logger;

        public GenerateReportAction(EtherClient client, NavigationManager uriHelper, IMatToaster toaster, ILogger<GenerateReportAction> logger)
        {
            _client = client;
            _navigation = uriHelper;
            _toaster = toaster;
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

                _toaster.Add("Report generated successfully", MatToastType.Success, "Report", MatIconNames.Done);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                _toaster.Add($"Error generating report: {ex.Message}", MatToastType.Danger, "Report", MatIconNames.Error);
            }
        }
    }
}

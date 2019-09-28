using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteReport : IAsyncAction<ReportViewModel>
    {
        private readonly EtherClient _client;
        private readonly JsUtils _jsUtils;

        public DeleteReport(EtherClient client, JsUtils jsUtils)
        {
            _client = client;
            _jsUtils = jsUtils;
        }

        public async Task Execute(IDispatcher dispatcher, ReportViewModel report)
        {
            try
            {
                await _client.Delete<ReportViewModel>(report.Id);
                await dispatcher.Dispatch<FetchReports>();
                await _jsUtils.NotifySuccess("Report deleted", $"{report.ReportType} was deleted successfully.");
            }
            catch (System.Exception ex)
            {
                await _jsUtils.NotifyError("Error deleting report", $"{ex.Message}");
            }
        }
    }
}

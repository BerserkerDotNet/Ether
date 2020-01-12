using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class DeleteReport : IAsyncAction<ReportViewModel>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public DeleteReport(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, ReportViewModel report)
        {
            try
            {
                await Utils.ExecuteWithLoading(dispatcher, async () =>
                {
                    await _client.Delete<ReportViewModel>(report.Id);
                });
                await dispatcher.Dispatch<FetchReports>();
                _toaster.Add($"{report.ReportType} was deleted successfully.", MatToastType.Success, "Report deleted", MatIconNames.Delete);
            }
            catch (System.Exception ex)
            {
                _toaster.Add(ex.Message, MatToastType.Danger, "Error deleting report", MatIconNames.Error);
            }
        }
    }
}

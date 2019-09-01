using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchReports : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchReports(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            var reports = await _client.GetAll<ReportViewModel>();
            dispatcher.Dispatch(new ReceiveReportsAction
            {
                Reports = reports
            });
        }
    }
}

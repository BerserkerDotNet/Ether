using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;

namespace Ether.Actions.Async
{
    public class FetchReportDescriptors : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchReportDescriptors(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            var reportDescriptors = await _client.GetReportTypes();
            dispatcher.Dispatch(new ReceiveReportDescriptorsAction
            {
                ReportDescriptors = reportDescriptors
            });
        }
    }
}

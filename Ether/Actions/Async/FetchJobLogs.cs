using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchJobLogs : IAsyncAction<FetchJobLogsCommand>
    {
        private readonly EtherClient _client;

        public FetchJobLogs(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, FetchJobLogsCommand command)
        {
            var page = await _client.GetAllPaged<JobLogViewModel>(command.Page, command.ItemsPerPage);
            dispatcher.Dispatch(new ReceivedJobLogsPage
            {
                Logs = page.Items,
                TotalPages = page.TotalPages,
                CurrentPage = page.CurrentPage
            });
        }
    }
}

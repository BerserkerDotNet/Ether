using System;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using Ether.ViewModels.Types;

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
            var page = await _client.GetAllPaged(command.Page, command.ItemsPerPage, o =>
            {
                var log = o.ToObject<JobLogViewModel>();
                if (log.JobType == "PullRequestsSyncJob")
                {
                    var details = o.GetValue(nameof(JobLogViewModel.Details), StringComparison.OrdinalIgnoreCase).ToObject<PullRequestJobDetails>();
                    log.Details = details;
                }

                return log;
            });

            dispatcher.Dispatch(new ReceivedJobLogsPage
            {
                Logs = page.Items,
                TotalPages = page.TotalPages,
                CurrentPage = page.CurrentPage
            });
        }
    }
}

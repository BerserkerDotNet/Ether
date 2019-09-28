using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using Ether.ViewModels.Types;

namespace Ether.Actions.Async
{
    public class FetchJobDetailsAction : IAsyncAction<JobLogViewModel>
    {
        private readonly EtherClient _client;

        public FetchJobDetailsAction(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, JobLogViewModel log)
        {
            var details = await _client.GetJobDetailsById<PullRequestJobDetails>(log.Id);
            dispatcher.Dispatch(new UpdateJobLogDetail
            {
                Details = details,
                JobLogId = log.Id
            });
        }
    }
}

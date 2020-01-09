using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class FetchPullRequests : IAsyncAction<FetchDataJobParameters>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public FetchPullRequests(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, FetchDataJobParameters parameters)
        {
            await _client.RunPullRequestsJob(parameters.Members, parameters.Reset);
            await dispatcher.Dispatch<FetchProfiles>();
            _toaster.Add($"Started to fetch pull requests for {parameters.Members.Count()} members.", MatToastType.Info, "Fetch Pull Requests", MatIconNames.Info);
        }
    }
}

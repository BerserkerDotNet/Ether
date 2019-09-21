using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;

namespace Ether.Actions.Async
{
    public class FetchPullRequests : IAsyncAction<FetchDataJobParameters>
    {
        private readonly EtherClient _client;
        private readonly JsUtils _jsUtils;

        public FetchPullRequests(EtherClient client, JsUtils jsUtils)
        {
            _client = client;
            _jsUtils = jsUtils;
        }

        public async Task Execute(IDispatcher dispatcher, FetchDataJobParameters parameters)
        {
            await _client.RunPullRequestsJob(parameters.Members, parameters.Reset);
            await dispatcher.Dispatch<FetchProfiles>();
            await _jsUtils.NotifySuccess("Fetch Workitems", $"Started to fetch workitems for {parameters.Members.Count()} members.");
        }
    }
}

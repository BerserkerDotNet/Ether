using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class FetchWorkItems : IAsyncAction<FetchDataJobParameters>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public FetchWorkItems(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, FetchDataJobParameters parameters)
        {
            _toaster.Add($"Started to fetch workitems for {parameters.Members.Count()} members.", MatToastType.Info, "Fetch Workitems", MatIconNames.Info);
            await _client.RunWorkitemsJob(parameters.Members, parameters.Reset);
            await dispatcher.Dispatch<FetchProfiles>();
        }
    }
}

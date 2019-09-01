using System;
using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.Types;

namespace Ether.Actions.Async
{
    public class FetchDataSourceSettings : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchDataSourceSettings(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            var config = await _client.GetVstsDataSourceConfig();
            dispatcher.Dispatch(new ReceiveDataSourceConfig
            {
                Config = config
            });
        }
    }
}

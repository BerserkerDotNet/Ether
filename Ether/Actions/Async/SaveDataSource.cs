using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveDataSource : IAsyncAction<VstsDataSourceViewModel>
    {
        private readonly EtherClient _client;

        public SaveDataSource(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, VstsDataSourceViewModel dataSource)
        {
            await _client.SaveVstsDataSourceConfig(dataSource);
            await dispatcher.Dispatch<FetchDataSourceSettings>();

        }
    }
}

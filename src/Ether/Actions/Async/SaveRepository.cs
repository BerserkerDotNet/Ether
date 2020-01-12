using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveRepository : IAsyncAction<VstsRepositoryViewModel>
    {
        private readonly EtherClient _client;

        public SaveRepository(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, VstsRepositoryViewModel repository)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                await _client.Save(repository);

                // TODO: instead of refresh insert?
                await dispatcher.Dispatch<FetchRepositories>();
            });
        }
    }
}

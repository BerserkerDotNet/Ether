using System.Threading.Tasks;
using Ether.Redux.Interfaces;
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
            await _client.Save(repository);
            // TODO: instead of refresh insert?
            await dispatcher.Dispatch<FetchRepositories>();
        }
    }
}

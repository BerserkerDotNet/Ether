using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchRepositories : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchRepositories(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                var repositories = await _client.GetAll<VstsRepositoryViewModel>();
                dispatcher.Dispatch(new ReceiveRepositoriesAction
                {
                    Repositories = repositories
                });
            });
        }
    }
}

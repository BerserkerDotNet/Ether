using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteRepository : IAsyncAction<VstsRepositoryViewModel>
    {
        private readonly EtherClient _client;
        private readonly JsUtils _jsUtils;

        public DeleteRepository(EtherClient client, JsUtils jsUtils)
        {
            _client = client;
            _jsUtils = jsUtils;
        }

        public async Task Execute(IDispatcher dispatcher, VstsRepositoryViewModel repository)
        {
            await _client.Delete<VstsRepositoryViewModel>(repository.Id);
            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchRepositories>();
            await _jsUtils.NotifySuccess("Delete", $"Repository {repository.Name} was deleted successfully.");
        }
    }
}

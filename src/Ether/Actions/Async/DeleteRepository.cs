using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class DeleteRepository : IAsyncAction<VstsRepositoryViewModel>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public DeleteRepository(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, VstsRepositoryViewModel repository)
        {
            await _client.Delete<VstsRepositoryViewModel>(repository.Id);

            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchRepositories>();
            _toaster.Add($"Repository {repository.Name} was deleted successfully.", MatToastType.Success, "Delete", MatIconNames.Delete);
        }
    }
}

using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;
using MatBlazor;

namespace Ether.Actions.Async
{
    public class DeleteProject : IAsyncAction<VstsProjectViewModel>
    {
        private readonly EtherClient _client;
        private readonly IMatToaster _toaster;

        public DeleteProject(EtherClient client, IMatToaster toaster)
        {
            _client = client;
            _toaster = toaster;
        }

        public async Task Execute(IDispatcher dispatcher, VstsProjectViewModel project)
        {
            await _client.Delete<VstsProjectViewModel>(project.Id);

            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchProjects>();
            _toaster.Add($"Project {project.Name} was deleted successfully.", MatToastType.Success, "Delete", MatIconNames.Delete);
        }
    }
}

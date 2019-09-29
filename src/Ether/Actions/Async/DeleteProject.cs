using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class DeleteProject : IAsyncAction<VstsProjectViewModel>
    {
        private readonly EtherClient _client;
        private readonly JsUtils _jsUtils;

        public DeleteProject(EtherClient client, JsUtils jsUtils)
        {
            _client = client;
            _jsUtils = jsUtils;
        }

        public async Task Execute(IDispatcher dispatcher, VstsProjectViewModel project)
        {
            await _client.Delete<VstsProjectViewModel>(project.Id);
            // TODO: instead of refresh delete?
            await dispatcher.Dispatch<FetchProjects>();
            await _jsUtils.NotifySuccess("Delete", $"Project {project.Name} was deleted successfully.");
        }
    }
}

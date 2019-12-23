using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class SaveProject : IAsyncAction<VstsProjectViewModel>
    {
        private readonly EtherClient _client;

        public SaveProject(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher, VstsProjectViewModel project)
        {
            await _client.Save(project);

            // TODO: instead of refresh insert?
            await dispatcher.Dispatch<FetchProjects>();
        }
    }
}

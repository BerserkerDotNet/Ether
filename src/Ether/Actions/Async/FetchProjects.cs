using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Types;
using Ether.ViewModels;

namespace Ether.Actions.Async
{
    public class FetchProjects : IAsyncAction
    {
        private readonly EtherClient _client;

        public FetchProjects(EtherClient client)
        {
            _client = client;
        }

        public async Task Execute(IDispatcher dispatcher)
        {
            await Utils.ExecuteWithLoading(dispatcher, async () =>
            {
                var projects = await _client.GetAll<VstsProjectViewModel>();
                dispatcher.Dispatch(new ReceiveProjectsAction
                {
                    Projects = projects
                });
            });
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ProjectsStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public ProjectsStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<VstsProjectViewModel> Projects => _state.Projects;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Projects = null;
            }

            if (_state.Projects == null)
            {
                _state.Projects = await _client.GetAll<VstsProjectViewModel>();
            }
        }

        public async Task UpdateAsync(VstsProjectViewModel project)
        {
            await _client.Save(project);
            if (!Projects.Any(p => p.Id == project.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteAsync(VstsProjectViewModel project)
        {
            await _client.Delete<VstsProjectViewModel>(project.Id);
            await LoadAsync(hard: true);
        }
    }
}

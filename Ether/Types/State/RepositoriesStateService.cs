using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class RepositoriesStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public RepositoriesStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<VstsRepositoryViewModel> Repositories => _state.Repositories;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Repositories = null;
            }

            if (_state.Repositories == null)
            {
                _state.Repositories = await _client.GetAll<VstsRepositoryViewModel>();
            }
        }

        public async Task UpdateAsync(VstsRepositoryViewModel repository)
        {
            await _client.Save(repository);
            if (!Repositories.Any(r => r.Id == repository.Id))
            {
                await LoadAsync(hard: true);
            }
        }

        public async Task DeleteAsync(VstsRepositoryViewModel repository)
        {
            await _client.Delete<VstsRepositoryViewModel>(repository.Id);
            await LoadAsync(hard: true);
        }
    }
}

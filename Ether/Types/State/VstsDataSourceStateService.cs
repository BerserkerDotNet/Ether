using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class VstsDataSourceStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public VstsDataSourceStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public VstsDataSourceViewModel VstsDataSource => _state.VstsDataSource;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard || VstsDataSource == null)
            {
                _state.VstsDataSource = await _client.GetVstsDataSourceConfig();
            }
        }

        public async Task UpdateAsync(VstsDataSourceViewModel model)
        {
            await _client.SaveVstsDataSourceConfig(model);
            _state.VstsDataSource = model;
        }
    }
}

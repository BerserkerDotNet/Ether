using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ReportStateService
    {
        private readonly EtherClient _client;
        private readonly AppState _state;

        public ReportStateService(EtherClient client, AppState state)
        {
            _client = client;
            _state = state;
        }

        public IEnumerable<ReportViewModel> Reports => _state.Reports;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Reports = null;
            }

            if (_state.Reports == null)
            {
                _state.Reports = await _client.GetAll<ReportViewModel>();
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class JobLogsStateService
    {
        private readonly EtherClient _client;
        private readonly AppState _state;

        public JobLogsStateService(EtherClient client, AppState state)
        {
            _client = client;
            _state = state;
        }

        public IEnumerable<JobLogViewModel> Logs => _state.JobLogs;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.JobLogs = null;
            }

            if (_state.JobLogs == null)
            {
                _state.JobLogs = await _client.GetAll<JobLogViewModel>();
            }
        }
    }
}

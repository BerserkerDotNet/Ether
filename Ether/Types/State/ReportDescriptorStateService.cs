using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ReportDescriptorStateService
    {
        private readonly EtherClient _client;
        private readonly AppState _state;

        public ReportDescriptorStateService(EtherClient client, AppState state)
        {
            _client = client;
            _state = state;
        }

        public IEnumerable<ReporterDescriptorViewModel> ReportTypes => _state.ReportTypes;

        public Dictionary<string, string> ReportTypeOptions
        {
            get
            {
                var types = ReportTypes ?? Enumerable.Empty<ReporterDescriptorViewModel>();
                var result = new Dictionary<string, string>(types.Count());
                foreach (var type in types)
                {
                    result.Add(type.UniqueName, type.DisplayName);
                }

                return result;
            }
        }

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.ReportTypes = null;
            }

            if (_state.ReportTypes == null)
            {
                _state.ReportTypes = await _client.GetReportTypes();
            }
        }
    }
}

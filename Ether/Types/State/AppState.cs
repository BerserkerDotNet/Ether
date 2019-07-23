using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class AppState
    {
        public IEnumerable<VstsProjectViewModel> Projects { get; set; }

        public IEnumerable<TeamMemberViewModel> TeamMembers { get; set; }

        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public IEnumerable<VstsRepositoryViewModel> Repositories { get; set; }

        public IEnumerable<IdentityViewModel> Identities { get; set; }

        public VstsDataSourceViewModel VstsDataSource { get; set; }

        public IEnumerable<JobLogViewModel> JobLogs { get; set; }

        public IEnumerable<ReportViewModel> Reports { get; set; }

        public IEnumerable<ReporterDescriptorViewModel> ReportTypes { get; set; }

        public Dictionary<Guid, DashboardState> DashboardState { get; set; }

        public IEnumerable<DashboardSettingsViewModel> Dashboards { get; set; }
    }

    public class DashboardState
    {
        public IEnumerable<WorkitemInformationViewModel> ActiveWorkitems { get; set; }
    }

    public class DashboardStateService
    {
        private readonly AppState _state;
        private readonly EtherClient _client;

        public DashboardStateService(AppState state, EtherClient client)
        {
            _state = state;
            _client = client;
        }

        public IEnumerable<DashboardSettingsViewModel> Dashboards => _state.Dashboards;

        //public IEnumerable<WorkitemInformationViewModel> GetActiveWorkItems(Guid id)
        //{
        //    if (_state.DashboardState.ContainsKey(id))
        //    {

        //    }
        //}

        public async Task LoadDashboardsAsync(bool hard = false)
        {
            if (hard)
            {
                _state.Dashboards = null;
            }

            if (_state.Dashboards == null)
            {
                _state.Dashboards = await _client.GetAll<DashboardSettingsViewModel>();
                _state.DashboardState = new Dictionary<Guid, DashboardState>(_state.Dashboards.Count());
            }
        }

        public async Task LoadDashboardAsync(Guid id, bool hard = false)
        {
            var dashBoardSettings = Dashboards.FirstOrDefault(d => d.Id == id);
            if (dashBoardSettings == null)
            {
                return;
            }

            var dashboardState = new DashboardState();
            var workitemVm = await _client.GetActiveWorkitems(dashBoardSettings.ProfileId);
            dashboardState.ActiveWorkitems = workitemVm.Workitems;
        }
    }
}

using System.Collections.Generic;
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
    }
}

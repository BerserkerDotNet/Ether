using System.Collections.Generic;

namespace Ether.Types.State
{
    public class RootState
    {
        public GenerateReportFormState GenerateReportForm { get; set; }

        public JobLogsState JobLogs { get; set; }

        public ProfilesState Profiles { get; set; }

        public SettingsState Settings { get; set; }

        public ReportsState Reports { get; set; }

        public TeamMembersState TeamMembers { get; set; }

        public RepositoriesState Repositories { get; set; }

        public ProjectsState Projects { get; set; }
    }
}

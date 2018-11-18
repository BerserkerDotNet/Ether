using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Vsts.Types
{
    public class RepositoryInfo
    {
        public string Name { get; set; }

        public IEnumerable<VstsTeamMemberViewModel> Members { get; set; }

        public ProjectInfo Project { get; set; }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }

        public bool IsWorkItemsEnabled { get; set; }

        public IdentityViewModel Identity { get; set; }
    }
}

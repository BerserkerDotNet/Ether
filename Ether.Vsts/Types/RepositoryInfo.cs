using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Vsts.Types
{
    public class RepositoryInfo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<TeamMemberViewModel> Members { get; set; }

        public ProjectInfo Project { get; set; }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }

        public bool IsWorkItemsEnabled { get; set; }

        public IdentityViewModel Identity { get; set; }
    }
}

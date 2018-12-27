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

    public class ProjectInfo : IEquatable<ProjectInfo>
    {
        public string Name { get; set; }

        public bool IsWorkItemsEnabled { get; set; }

        public IdentityViewModel Identity { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProjectInfo);
        }

        public bool Equals(ProjectInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

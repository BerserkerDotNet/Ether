using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class ProjectsState
    {
        public ProjectsState(IEnumerable<VstsProjectViewModel> projects)
        {
            Projects = projects;
        }

        public IEnumerable<VstsProjectViewModel> Projects { get; private set; }
    }
}

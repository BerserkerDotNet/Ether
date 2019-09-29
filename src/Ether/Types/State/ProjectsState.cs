using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class ProjectsState
    {
        [JsonConstructor]
        public ProjectsState(IEnumerable<VstsProjectViewModel> projects)
        {
            Projects = projects;
        }

        public IEnumerable<VstsProjectViewModel> Projects { get; private set; }
    }
}

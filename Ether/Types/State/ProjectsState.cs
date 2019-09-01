using Ether.ViewModels;
using System.Collections.Generic;

namespace Ether.Types.State
{
    public class ProjectsState
    {
        public IEnumerable<VstsProjectViewModel> Projects { get; set; }
    }
}

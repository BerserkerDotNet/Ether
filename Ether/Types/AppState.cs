using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types
{
    public class AppState
    {
        public IReadOnlyList<VstsProjectViewModel> Projects { get; set; }
    }
}

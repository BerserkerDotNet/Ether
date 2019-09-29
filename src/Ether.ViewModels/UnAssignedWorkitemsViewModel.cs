using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class UnAssignedWorkitemsViewModel
    {
        public UnAssignedWorkitemsViewModel()
        {
            Workitems = Enumerable.Empty<WorkitemInformationViewModel>();
        }

        public IEnumerable<WorkitemInformationViewModel> Workitems { get; set; }
    }
}

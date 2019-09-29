using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class ActiveWorkitemsViewModel
    {
        public ActiveWorkitemsViewModel()
        {
            Workitems = Enumerable.Empty<WorkitemInformationViewModel>();
        }

        public IEnumerable<WorkitemInformationViewModel> Workitems { get; set; }
    }
}

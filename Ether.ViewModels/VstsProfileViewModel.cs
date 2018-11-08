using System;
using System.Collections.Generic;

namespace Ether.ViewModels
{
    public class VstsProfileViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public IEnumerable<Guid> Members { get; set; }

        public IEnumerable<Guid> Repositories { get; set; }
    }
}

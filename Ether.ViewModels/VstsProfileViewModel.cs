using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class VstsProfileViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public IEnumerable<Guid> Members { get; set; } = Enumerable.Empty<Guid>();

        public IEnumerable<Guid> Repositories { get; set; } = Enumerable.Empty<Guid>();
    }
}

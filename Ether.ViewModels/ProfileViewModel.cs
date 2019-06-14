using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class ProfileViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public string Type { get; set; } = "Vsts";

        public IEnumerable<Guid> Members { get; set; }

        public IEnumerable<Guid> Repositories { get; set; }
    }
}

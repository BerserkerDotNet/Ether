using System;

namespace Ether.ViewModels
{
    public class VstsRepositoryViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public Guid Project { get; set; }
    }
}

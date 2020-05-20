using System;

namespace Ether.ViewModels
{
    public class VstsProjectViewModel : ViewModelWithId
    {
        public string Name { get; set; }

        public bool IsWorkItemsEnabled { get; set; }

        public Guid Organization { get; set; }

        public Guid Identity { get; set; }
    }
}

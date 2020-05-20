using System;

namespace Ether.ViewModels
{
    public class OrganizationViewModel : ViewModelWithId
    {
        public Guid Identity { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}

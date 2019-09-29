using System;

namespace Ether.ViewModels
{
    public class VstsDataSourceViewModel : ViewModelWithId
    {
        public Guid? DefaultToken { get; set; }

        public string InstanceName { get; set; }
    }
}

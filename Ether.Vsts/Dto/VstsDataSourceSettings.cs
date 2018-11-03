using Ether.Contracts.Attributes;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    [DbName(nameof(DataSourceSettings))]
    public class VstsDataSourceSettings : DataSourceSettings
    {
        public VstsDataSourceSettings()
            : base(Constants.VstsDataSourceType)
        {
        }

        public string DefaultToken { get; set; }

        public string InstanceName { get; set; }
    }
}

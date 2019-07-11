using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetDashboardSettings : IQuery<DashboardSettingsViewModel>
    {
        public GetDashboardSettings(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}

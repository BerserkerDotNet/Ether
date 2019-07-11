using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;

namespace Ether.Types.EditableTable
{
    public class DashboardSubTeamEditableTableDataProvider : EtherClientEditableTableDataProvider
    {
        private readonly Guid _dashboardId;

        public DashboardSubTeamEditableTableDataProvider(EtherClient client, Guid dashboardId)
            : base(client)
        {
            _dashboardId = dashboardId;
        }

        public override async Task<IEnumerable<T>> Load<T>()
        {
            Console.WriteLine("Provider load");
            var settings = await GetSettings();

            Console.WriteLine("Loaded: " + settings.SubTeams == null);
            return (IEnumerable<T>)settings.SubTeams;
        }

        public override async Task Save<T>(T item)
        {
            var settings = await GetSettings();
            var subFilter = item as FilterSubTeam;
            var itemToUpdate = settings.SubTeams.SingleOrDefault(t => t.Id == subFilter.Id);
            if (itemToUpdate == null)
            {
                settings.SubTeams = settings.SubTeams.Union(new[] { subFilter });
            }
            else
            {
                itemToUpdate.Members = subFilter.Members;
                itemToUpdate.Name = subFilter.Name;
            }

            await base.Save(settings);
        }

        public override async Task Delete<T>(Guid id)
        {
            var settings = await GetSettings();
            settings.SubTeams = settings.SubTeams.Where(t => t.Id != id);

            await base.Save(settings);
        }

        private async Task<DashboardSettingsViewModel> GetSettings() => await Client.GetById<DashboardSettingsViewModel>(_dashboardId);
    }
}

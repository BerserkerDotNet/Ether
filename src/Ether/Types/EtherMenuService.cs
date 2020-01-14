using System.Collections.Generic;
using System.Threading.Tasks;
using MatBlazor;

namespace Ether.Types
{
    public class EtherMenuService
    {
        private readonly EtherClient _client;

        public EtherMenuService(EtherClient client)
        {
            _client = client;
        }

        public IList<MenuItem> Menu { get; private set; }

        public async Task LoadMenuAsync()
        {
            Menu = new List<MenuItem>(4);
            await AddItem(MenuItem.Create("Home", MatIconNames.Home, "/", "Reports"));
            //await AddItem(MenuItem.Create("Dashboard", MatIconNames.Dashboard, "dashboard", "Dashboard"));
            await AddItem(MenuItem.Create("Reports", MatIconNames.Pie_chart, "reports", "Reports"));
            await AddItem(MenuItem.CreateContainer("Azure DevOps", MatIconNames.Cloud, "AzureDevOps Settings",
                MenuItem.Create("Profiles", MatIconNames.View_list, "azure-devops/profiles", "AzureDevOps Settings"),
                MenuItem.Create("Team Members", MatIconNames.People, "azure-devops/teammembers", "AzureDevOps Settings"),
                MenuItem.Create("Projects & Repositories", MatIconNames.Bar_chart, "azure-devops/projects-and-repositories", "AzureDevOps Settings")));
            await AddItem(MenuItem.CreateContainer("Settings", MatIconNames.Settings, "Settings",
                MenuItem.Create("Settings", MatIconNames.Settings_applications, "settings", "Settings"),
                MenuItem.Create("Job Logs", MatIconNames.Book, "job-logs", "Logs")));
        }

        private async Task AddItem(MenuItem item)
        {
            var hasAccess = await _client.IsUserHasAccess(item.Path, item.Category);
            if (!hasAccess)
            {
                return;
            }

            Menu.Add(item);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

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
            await AddItem(MenuItem.Create("Home", "home", string.Empty, "Reports"));
            await AddItem(MenuItem.Create("Reports", "chart-pie", "reports", "Reports"));
            await AddItem(MenuItem.CreateContainer("Azure DevOps", "cloud", "AzureDevOps Settings",
                MenuItem.Create("Profiles", "address-card", "azure-devops/profiles", "AzureDevOps Settings"),
                MenuItem.Create("Team Members", "users", "azure-devops/teammembers", "AzureDevOps Settings"),
                MenuItem.Create("Projects & Repositories", "project-diagram", "azure-devops/projects-and-repositories", "AzureDevOps Settings")));
            await AddItem(MenuItem.CreateContainer("Settings", "cogs", "Settings",
                MenuItem.Create("Settings", "cog", "settings", "Settings"),
                MenuItem.Create("Job Logs", "book", "job-logs", "Logs")));
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

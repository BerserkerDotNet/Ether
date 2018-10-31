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
            Menu = new List<MenuItem>(3);
            await AddItem(new MenuItem("Home", "paper-2", string.Empty, "Reports"));
            await AddItem(new MenuItem("Reports", "single-copy-04", "reports", "Reports"));
            await AddItem(new MenuItem("Settings", "single-02", "settings", "Settings"));
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

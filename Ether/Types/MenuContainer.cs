using System.Collections.Generic;

namespace Ether.Types
{
    public class MenuContainer : MenuItem
    {
        public MenuContainer(string name, string icon, string category)
            : base(name, icon, string.Empty, category)
        {
            Children = new List<MenuItem>();
        }

        public IList<MenuItem> Children { get; set; }
    }
}

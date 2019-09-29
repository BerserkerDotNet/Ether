using System;
using System.Collections.Generic;

namespace Ether.Types
{
    public class MenuItem
    {
        private MenuItem(string name, string icon, string path, string category)
        {
            Name = name;
            Icon = icon;
            Path = path;
            Category = category;
        }

        public string Name { get; }

        public string Icon { get; }

        public string Path { get; }

        public string Category { get; set; }

        public List<MenuItem> Children { get; set; } = new List<MenuItem>();

        public static MenuItem Create(string name, string icon, string path, string category)
        {
            return new MenuItem(name, icon, path, category);
        }

        public static MenuItem CreateContainer(string name, string icon, string category, params MenuItem[] children)
        {
            var menu = new MenuItem(name, icon, string.Empty, category);
            menu.Children.AddRange(children);
            return menu;
        }
    }
}

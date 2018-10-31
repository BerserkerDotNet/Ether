namespace Ether.Types
{
    public class MenuItem
    {
        public MenuItem(string name, string icon, string path, string category)
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
    }
}

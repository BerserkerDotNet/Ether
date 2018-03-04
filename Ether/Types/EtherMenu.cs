namespace Ether.Types
{
    public static class EtherMenu
    {
        private static MenuItem _root;

        static EtherMenu()
        {
            _root = new MenuContainer(string.Empty);
            _root.AddSubItem(new MenuItem(typeof(Pages.Home.IndexModel)));
            _root.AddSubItem(new MenuItem(typeof(Pages.Reports.IndexModel)));
            _root.AddSubItem(new MenuItem(typeof(Pages.Queries.IndexModel)));
            _root.AddSubItem(new MenuItem(typeof(Pages.Profiles.IndexModel)));
            var settingsContainer = new MenuContainer("Settings");
            settingsContainer.AddSubItem(new MenuItem(typeof(Pages.Settings.IndexModel)));
            settingsContainer.AddSubItem(new MenuItem(typeof(Pages.Settings.TeamMembersModel)));
            settingsContainer.AddSubItem(new MenuItem(typeof(Pages.Settings.ProjectsModel)));
            settingsContainer.AddSubItem(new MenuItem(typeof(Pages.Settings.RepositoriesModel)));
            _root.AddSubItem(settingsContainer);
        }

        public static MenuItem Menu => _root;
    }
}

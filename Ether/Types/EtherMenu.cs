using System;

namespace Ether.Types
{
    public static class EtherMenu
    {
        private static MenuItem _root;

        static EtherMenu()
        {
            _root = new MenuContainer(string.Empty);
            AddHomeSection();
            AddReportsSection();
            AddQueriesSection();
            AddProfilesSection();
            AddSettingsSection();
        }

        public static MenuItem Menu => _root;

        private static void AddHomeSection()
        {
            _root.AddSubItem(new MenuItem(typeof(Pages.Home.IndexModel)));
        }

        private static void AddReportsSection()
        {
            var item = new MenuItem(typeof(Pages.Reports.IndexModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Reports.ViewModel), isVisible: false));
            _root.AddSubItem(item);
        }

        private static void AddQueriesSection()
        {
            var item = new MenuItem(typeof(Pages.Queries.IndexModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Queries.HistoryModel), isVisible: false));
            item.AddSubItem(new MenuItem(typeof(Pages.Queries.EditModel), isVisible: false));
            _root.AddSubItem(item);
        }

        private static void AddProfilesSection()
        {
            var item = new MenuItem(typeof(Pages.Profiles.IndexModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Profiles.EditModel), isVisible: false));
            _root.AddSubItem(item);
        }

        private static void AddTeamMembersSection(MenuContainer settingsContainer)
        {
            var item = new MenuItem(typeof(Pages.Settings.TeamMembersModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Settings.EditMemberModel)));
            settingsContainer.AddSubItem(item);
        }

        private static void AddProjectsSection(MenuContainer settingsContainer)
        {
            var item = new MenuItem(typeof(Pages.Settings.ProjectsModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Settings.EditProjectModel)));
            settingsContainer.AddSubItem(item);
        }

        private static void AddRepositoriesSection(MenuContainer settingsContainer)
        {
            var item = new MenuItem(typeof(Pages.Settings.RepositoriesModel));
            item.AddSubItem(new MenuItem(typeof(Pages.Settings.EditRepositoryModel)));
            settingsContainer.AddSubItem(item);
        }

        private static void AddSettingsSection()
        {
            var settingsContainer = new MenuContainer("Settings");
            settingsContainer.AddSubItem(new MenuItem(typeof(Pages.Settings.IndexModel)));
            AddTeamMembersSection(settingsContainer);
            AddProjectsSection(settingsContainer);
            AddRepositoriesSection(settingsContainer);
            _root.AddSubItem(settingsContainer);
        }
    }
}

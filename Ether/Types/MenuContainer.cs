namespace Ether.Types
{
    public class MenuContainer : MenuItem
    {
        public MenuContainer(string title, string icon = "") 
            : base(pageType: null, icon: icon)
        {
            Title = title;
        }

        public bool IsRoot => string.IsNullOrEmpty(Title) && PageType == null;
    }
}

namespace Ether.Types
{
    public class MenuContainer : MenuItem
    {
        public MenuContainer(string title) 
            : base(null)
        {
            Title = title;
        }

        public bool IsRoot => string.IsNullOrEmpty(Title) && PageType == null;
    }
}

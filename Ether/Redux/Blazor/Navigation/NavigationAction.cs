using Ether.Redux.Interfaces;

namespace Ether.Redux.Blazor.Navigation
{
    public class NavigationAction : IAction
    {
        public string Url { get; set; }
    }
}

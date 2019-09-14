using Ether.Redux.Interfaces;

namespace Ether.Redux.Blazor.Navigation
{
    public class UserNavigationReducer : IReducer<string>
    {
        public string Reduce(string state, IAction action)
        {
            switch (action)
            {
                case NavigationAction a:
                    return a.Url;
                default:
                    return state;
            }
        }
    }
}

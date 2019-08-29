using Ether.Redux.Interfaces;

namespace Ether.Actions
{
    public class JobLogsMoveToPage : IAction
    {
        public int CurrentPage { get; set; }
    }
}

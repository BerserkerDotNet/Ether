using BlazorState.Redux.Interfaces;

namespace Ether.Actions
{
    public class Loading : IAction
    {
        public bool IsDone { get; set; }
    }
}

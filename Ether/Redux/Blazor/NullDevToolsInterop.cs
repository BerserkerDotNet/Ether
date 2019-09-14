using System;
using System.Threading.Tasks;
using Ether.Redux.Interfaces;

namespace Ether.Redux.Blazor
{
    public class NullDevToolsInterop : IDevToolsInterop
    {
        public event EventHandler<JumpToStateEventArgs> OnJumpToStateChanged;

        public ValueTask Init(object state)
        {
            return new ValueTask(Task.CompletedTask);
        }

        public void ReceiveMessage(DevToolsMessage message)
        {
        }

        public ValueTask Send(IAction action, object state)
        {
            return new ValueTask(Task.CompletedTask);
        }
    }
}

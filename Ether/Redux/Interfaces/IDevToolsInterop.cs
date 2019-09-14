using System;
using System.Threading.Tasks;
using Ether.Redux.Blazor;

namespace Ether.Redux.Interfaces
{
    public interface IDevToolsInterop
    {
        event EventHandler<JumpToStateEventArgs> OnJumpToStateChanged;

        ValueTask Init(object state);

        ValueTask Send(IAction action, object state);

        void ReceiveMessage(DevToolsMessage message);
    }
}

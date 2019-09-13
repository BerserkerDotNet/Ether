using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Microsoft.JSInterop;

namespace Ether.Redux.Blazor
{
    public class ReduxDevToolsInterop
    {
        private readonly IJSRuntime _jSRuntime;

        public ReduxDevToolsInterop(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public ValueTask<object> Init(object state)
        {
            return _jSRuntime.InvokeAsync<object>("window.BlazorRedux.sendInitial", state);
        }

        public ValueTask<object> Send(IAction action, object state)
        {
            return _jSRuntime.InvokeAsync<object>("window.BlazorRedux.send", action.ToString(), action , state);
        }
    }
}

using Ether.Redux.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Ether.Redux.Blazor
{
    public class ReduxDevToolsInterop
    {
        private readonly IJSRuntime _jSRuntime;

        public ReduxDevToolsInterop(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public Task Init(object state)
        {
            return _jSRuntime.InvokeAsync<object>("window.BlazorRedux.sendInitial", state);
        }

        public Task Send(IAction action, object state)
        {
            return _jSRuntime.InvokeAsync<object>("window.BlazorRedux.send", action.ToString(), action , state);
        }
    }
}

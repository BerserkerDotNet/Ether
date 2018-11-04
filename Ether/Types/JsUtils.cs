using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Ether.Types
{
    public static class JsUtils
    {
        public static Task<bool> Confirm(string message)
        {
            return JSRuntime.Current.InvokeAsync<bool>("confirm", message);
        }
    }
}

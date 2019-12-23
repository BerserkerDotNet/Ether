using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Ether.Storage
{
    public class LocalStorage : ILocalStorage
    {
        private readonly IJSRuntime _jSRuntime;

        public LocalStorage(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public async ValueTask<T> GetItem<T>(string key)
        {
            var data = await _jSRuntime.InvokeAsync<string>("window.localStorage.getItem", key);
            return JsonConvert.DeserializeObject<T>(data);
        }

        public ValueTask SetItem<T>(string key, T value)
        {
            return _jSRuntime.InvokeVoidAsync("window.localStorage.setItem", key, value);
        }
    }
}

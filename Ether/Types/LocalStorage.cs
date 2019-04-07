using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Ether.Types
{
    public class LocalStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task<int> Length() => _jsRuntime.InvokeAsync<int>(MethodNames.LENGTH_METHOD, StorageTypeNames.LOCAL_STORAGE);

        public Task Clear() => _jsRuntime.InvokeAsync<object>(MethodNames.CLEAR_METHOD, StorageTypeNames.LOCAL_STORAGE);

        public Task<TItem> GetItem<TItem>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _jsRuntime.InvokeAsync<TItem>(MethodNames.GET_ITEM_METHOD, StorageTypeNames.LOCAL_STORAGE, key);
        }

        public Task<string> Key(int index) => _jsRuntime.InvokeAsync<string>(MethodNames.KEY_METHOD, StorageTypeNames.LOCAL_STORAGE, index);

        public Task RemoveItem(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _jsRuntime.InvokeAsync<object>(MethodNames.REMOVE_ITEM_METHOD, StorageTypeNames.LOCAL_STORAGE, key);
        }

        public Task SetItem<TItem>(string key, TItem item)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _jsRuntime.InvokeAsync<object>(MethodNames.SET_ITEM_METHOD, StorageTypeNames.LOCAL_STORAGE, key, item);
        }
    }

    internal class MethodNames
    {
        public const string LENGTH_METHOD = "BlazorExtensions.Storage.Length";
        public const string KEY_METHOD = "BlazorExtensions.Storage.Key";
        public const string GET_ITEM_METHOD = "BlazorExtensions.Storage.GetItem";
        public const string SET_ITEM_METHOD = "BlazorExtensions.Storage.SetItem";
        public const string REMOVE_ITEM_METHOD = "BlazorExtensions.Storage.RemoveItem";
        public const string CLEAR_METHOD = "BlazorExtensions.Storage.Clear";
    }

    internal class StorageTypeNames
    {
        public const string SESSION_STORAGE = "sessionStorage";
        public const string LOCAL_STORAGE = "localStorage";
    }
}

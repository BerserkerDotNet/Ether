using System;
using System.Threading.Tasks;
using Ether.Components.Form;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ether.Types
{
    public class JsUtils
    {
        private readonly IJSRuntime _jsRuntime;

        public JsUtils(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask Print()
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.print");
        }

        public ValueTask SaveAsFile(string fileName, string base64EncodedFile)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.saveAsFile", fileName, base64EncodedFile);
        }

        public ValueTask DateRangePicker(ElementReference element, DotNetObjectReference<object> component)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.DateRangePicker.init", element, component);
        }

        public ValueTask SummerNoteBootstrap(ElementReference element, string value, DotNetObjectReference<object> component)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.SummerNoteBootstrap.init", element, value, component);
        }
    }
}

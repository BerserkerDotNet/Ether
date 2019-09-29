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

        public ValueTask<bool> Confirm(string message)
        {
            return _jsRuntime.InvokeAsync<bool>("confirm", message);
        }

        public ValueTask DataTable(ElementReference tableRef)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.DataTableInterop.initializeDataTable", tableRef);
        }

        public ValueTask<string[]> GetAllSelectedOptions(ElementReference selectRef)
        {
            return _jsRuntime.InvokeAsync<string[]>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public ValueTask<string> GetSelectedOption(ElementReference selectRef)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public ValueTask FailValidation(string id)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.failValidation", id);
        }

        public ValueTask SucceedValidation(string id)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.succeedValidation", id);
        }

        public ValueTask ButtonState(ElementReference btn, string state)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.buttonState", btn, state);
        }

        public ValueTask Print()
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.print");
        }

        public ValueTask SaveAsFile(string fileName, string base64EncodedFile)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Utils.saveAsFile", fileName, base64EncodedFile);
        }

        public ValueTask NotifyError(string title, string message)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Notify.notify", "danger", title, message, 8000);
        }

        public ValueTask NotifySuccess(string title, string message)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.Notify.notify", "success", title, message, 3000);
        }

        public ValueTask DateRangePicker(ElementReference element, DotNetObjectReference<object> component)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.DateRangePicker.init", element, component);
        }

        public ValueTask SummerNoteBootstrap(ElementReference element, string value, DotNetObjectReference<object> component)
        {
            return _jsRuntime.InvokeVoidAsync("window.BlazorComponents.SummerNoteBootstrap.init", element, value, component);
        }

        public ValueTask<DateTime> GetDatePickerStartDate(ElementReference element)
        {
            return _jsRuntime.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getStartDate", element);
        }

        public ValueTask<DateTime> GetDatePickerEndDate(ElementReference element)
        {
            return _jsRuntime.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getEndDate", element);
        }

        public ValueTask<string> GetValue(ElementReference element)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.getValue", element);
        }

        public ValueTask<string> SetValue(ElementReference element, string value)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.setValue", element, value);
        }
    }
}

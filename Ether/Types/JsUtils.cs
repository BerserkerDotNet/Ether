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

        public Task<bool> Confirm(string message)
        {
            return _jsRuntime.InvokeAsync<bool>("confirm", message);
        }

        public Task DataTable(ElementRef tableRef)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.DataTableInterop.initializeDataTable", tableRef);
        }

        public Task<string[]> GetAllSelectedOptions(ElementRef selectRef)
        {
            return _jsRuntime.InvokeAsync<string[]>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public Task<string> GetSelectedOption(ElementRef selectRef)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public Task FailValidation(string id)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.failValidation", id);
        }

        public Task SucceedValidation(string id)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.succeedValidation", id);
        }

        public Task ButtonState(ElementRef btn, string state)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.buttonState", btn, state);
        }

        public Task Print()
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.Utils.print");
        }

        public Task SaveAsFile(string fileName, string base64EncodedFile)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.Utils.saveAsFile", fileName, base64EncodedFile);
        }

        public Task NotifyError(string title, string message)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.Notify.notify", "danger", title, message, 8000);
        }

        public Task NotifySuccess(string title, string message)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.Notify.notify", "success", title, message, 3000);
        }

        public Task DateRangePicker(ElementRef element, DotNetObjectRef<object> component)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.DateRangePicker.init", element, component);
        }

        public Task SummerNoteBootstrap(ElementRef element, string value, DotNetObjectRef<object> component)
        {
            return _jsRuntime.InvokeAsync<object>("window.BlazorComponents.SummerNoteBootstrap.init", element, value, component);
        }

        public Task<DateTime> GetDatePickerStartDate(ElementRef element)
        {
            return _jsRuntime.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getStartDate", element);
        }

        public Task<DateTime> GetDatePickerEndDate(ElementRef element)
        {
            return _jsRuntime.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getEndDate", element);
        }

        public Task<string> GetValue(ElementRef element)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.getValue", element);
        }

        public Task<string> SetValue(ElementRef element, string value)
        {
            return _jsRuntime.InvokeAsync<string>("window.BlazorComponents.Utils.setValue", element, value);
        }
    }
}

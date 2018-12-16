using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;

namespace Ether.Types
{
    public static class JsUtils
    {
        public static Task<bool> Confirm(string message)
        {
            return JSRuntime.Current.InvokeAsync<bool>("confirm", message);
        }

        public static Task DataTable(ElementRef tableRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("window.BlazorComponents.DataTableInterop.initializeDataTable", tableRef);
        }

        public static Task<string[]> GetAllSelectedOptions(ElementRef selectRef)
        {
            return JSRuntime.Current.InvokeAsync<string[]>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public static Task<string> GetSelectedOption(ElementRef selectRef)
        {
            return JSRuntime.Current.InvokeAsync<string>("window.BlazorComponents.Utils.getAllSelectedOptions", selectRef);
        }

        public static Task FailValidation(string id)
        {
            return JSRuntime.Current.InvokeAsync<string>("window.BlazorComponents.Utils.failValidation", id);
        }

        public static Task SucceedValidation(string id)
        {
            return JSRuntime.Current.InvokeAsync<string>("window.BlazorComponents.Utils.succeedValidation", id);
        }

        public static Task ButtonState(ElementRef btn, string state)
        {
            return JSRuntime.Current.InvokeAsync<string>("window.BlazorComponents.Utils.buttonState", btn, state);
        }

        public static Task Print()
        {
            return JSRuntime.Current.InvokeAsync<object>("window.BlazorComponents.Utils.print");
        }

        public static Task DateRangePicker(ElementRef element, DotNetObjectRef component)
        {
            return JSRuntime.Current.InvokeAsync<object>("window.BlazorComponents.DateRangePicker.init", element, component);
        }

        public static Task<DateTime> GetDatePickerStartDate(ElementRef element)
        {
            return JSRuntime.Current.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getStartDate", element);
        }

        public static Task<DateTime> GetDatePickerEndDate(ElementRef element)
        {
            return JSRuntime.Current.InvokeAsync<DateTime>("window.BlazorComponents.DateRangePicker.getEndDate", element);
        }
    }
}

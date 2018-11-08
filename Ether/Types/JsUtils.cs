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
            return JSRuntime.Current.InvokeAsync<object>("window.BlazerComponents.DataTableInterop.initializeDataTable", tableRef);
        }

        public static Task<string[]> GetAllSelectedOptions(ElementRef selectRef)
        {
            return JSRuntime.Current.InvokeAsync<string[]>("window.BlazerComponents.Utils.getAllSelectedOptions", selectRef);
        }
    }
}

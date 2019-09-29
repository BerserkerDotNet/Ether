using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Dashboard
{
    public class ActiveWorkitemsFilterBase : ComponentBase
    {
        [Parameter]
        public DashboardSettingsViewModel Settings { get; set; }

        [Parameter]
        public Guid DashboardId { get; set; }

        [Parameter]
        public Dictionary<string, string> TeamMebmersOptions { get; set; }

        [Parameter]
        public EventCallback<object> OnFilter { get; set; }

        protected async Task OnChange(ChangeEventArgs args)
        {
            await OnFilter.InvokeAsync(args.Value);
        }
    }
}

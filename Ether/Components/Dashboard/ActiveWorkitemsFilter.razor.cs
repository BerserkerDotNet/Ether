using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBootstrap.Modal.Services;
using Ether.Components.Code;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Dashboard
{
    public class ActiveWorkitemsFilterBase : ComponentBase
    {
        [Inject]
        protected IModal Modal { get; set; }

        [Parameter]
        protected DashboardSettingsViewModel Settings { get; private set; }

        [Parameter]
        protected Guid DashboardId { get; private set; }

        [Parameter]
        protected Dictionary<string, string> TeamMebmersOptions { get; set; }

        [Parameter]
        protected EventCallback<object> OnFilter { get; set; }

        protected void ShowDashboardSettings()
        {
            Modal.Show<DashboardSettings>("Settings", ModalParameter.Get("DashboardId", DashboardId));
        }

        protected async Task OnChange(UIChangeEventArgs args)
        {
            await OnFilter.InvokeAsync(args.Value);
        }
    }
}

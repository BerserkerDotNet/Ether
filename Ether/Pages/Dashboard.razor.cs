using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBootstrap.Modal.Services;
using Ether.Components.Code;
using Ether.Components.Dashboard;
using Ether.Types;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Pages
{
    public class DashboardBase : ComponentBase
    {
        protected Guid dashboardId = Guid.Parse("5441900f-d2e2-4b66-a4ac-8713641495b7");

        public DashboardBase()
        {
            SelectedTeamMember = string.Empty;
            TeamMebmersOptions = new Dictionary<string, string>();
            OrderByConfig = OrderByConfigurationBuilder<WorkitemInformationViewModel>.New()
                                .OrderBy(w => w.AssignedTo.Title)
                                .OrderByDescending(w => w.IsBlocked)
                                .OrderByDescending(w => w.IsOnHold)
                                .Build();
            Dashboards = new List<DashboardSettingsViewModel>(0);
        }

        protected bool IsLoading { get; private set; }

        protected DashboardSettingsViewModel Settings { get; private set; }

        protected List<DashboardSettingsViewModel> Dashboards { get; private set; }

        protected ActiveWorkitemsViewModel Model { get; private set; }

        protected string SelectedTeamMember { get; private set; }

        protected Dictionary<string, string> TeamMebmersOptions { get; private set; }

        protected OrderByConfiguration<WorkitemInformationViewModel>[] OrderByConfig { get; private set; }

        [Inject]
        protected EtherClient Client { get; set; }

        [Inject]
        protected IModal Modal { get; set; }

        protected override async Task OnInitAsync()
        {
            IsLoading = true;
            Dashboards = new List<DashboardSettingsViewModel>(await Client.GetAll<DashboardSettingsViewModel>());
            Settings = await Client.GetById<DashboardSettingsViewModel>(dashboardId);
            Model = await Client.GetActiveWorkitems(Guid.Parse("b79159f4-a834-49fb-9702-36076c664ea0"));
            TeamMebmersOptions = Model.Workitems.GroupBy(w => w.AssignedTo.Email).ToDictionary(k => k.Key, v => v.First().AssignedTo.Title);
            IsLoading = false;
        }

        protected string GetRowClass(WorkitemInformationViewModel item)
        {
            if (item.IsBlocked)
            {
                return "table-danger";
            }

            if (item.IsOnHold)
            {
                return "table-warning";
            }

            if (string.Equals(item.State, "active", StringComparison.OrdinalIgnoreCase))
            {
                return "table-primary";
            }

            if (string.Equals(item.State, "new", StringComparison.OrdinalIgnoreCase))
            {
                return "table-secondary";
            }

            if (string.Equals(item.State, "resolved", StringComparison.OrdinalIgnoreCase))
            {
                return "table-success";
            }

            return string.Empty;
        }

        protected void ApplyFilter(object value, DataTableBase<WorkitemInformationViewModel> dt)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                // TODO: Reset filter?
                dt.Filter(null);
            }
            else
            {
                var isGuid = Guid.TryParse(value.ToString(), out var id);
                if (isGuid)
                {
                    var members = Settings.SubTeams.SingleOrDefault(s => s.Id == id)?.Members;
                    dt.Filter(i => members != null && members.Any(m => string.Equals(i.AssignedTo.Email, m)));
                }
                else
                {
                    dt.Filter(i => string.Equals(i.AssignedTo.Email, value));
                }
            }
        }

        protected void ShowDashboardSettings(Guid id)
        {
            Modal.Show<DashboardSettings>("Settings", ModalParameter.Get("DashboardId", id));
        }

        protected void ShowDashboardSettings()
        {
            Modal.Show<DashboardSettings>("Settings");
        }
    }
}

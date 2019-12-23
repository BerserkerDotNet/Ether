using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorState.Redux.Blazor;
using BlazorState.Redux.Interfaces;
using Ether.Actions.Async;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Components.Reports
{
    public class ReportsListProps
    {
        public IEnumerable<ReportViewModel> Reports { get; set; }

        public EventCallback OnRefresh { get; set; }

        public EventCallback<ReportViewModel> OnDelete { get; set; }

        public EventCallback<ReportViewModel> OnView { get; set; }
    }

    public class ReportsListConnected
    {
        public static RenderFragment Get()
        {
            var c = new ReportsListConnected();
            return ComponentConnector.Connect<ReportsList, RootState, ReportsListProps>(c.MapStateToProps, c.MapDispatchToProps, c.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsReportsSstateInitialized(store.State))
            {
                await store.Dispatch<FetchReports>();
            }
        }

        private void MapStateToProps(RootState state, ReportsListProps props)
        {
            props.Reports = GetReports(state);
        }

        private void MapDispatchToProps(IStore<RootState> store, ReportsListProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnDelete = EventCallback.Factory.Create<ReportViewModel>(this, r => HandleDelete(store, r));
            props.OnView = EventCallback.Factory.Create<ReportViewModel>(this, r => HandleView(store, r));
        }

        private IEnumerable<ReportViewModel> GetReports(RootState state)
        {
            return state?.Reports?.Reports ?? null;
        }

        private bool IsReportsSstateInitialized(RootState state)
        {
            return state?.Reports?.Reports != null;
        }

        private async Task HandleDelete(IStore<RootState> store, ReportViewModel report)
        {
            await store.Dispatch<DeleteReport, ReportViewModel>(report);
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            await store.Dispatch<FetchReports>();
        }

        private async Task HandleView(IStore<RootState> store, ReportViewModel report)
        {
            await store.Dispatch<ViewReport, ReportViewModel>(report);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Actions;
using Ether.Actions.Async;
using Ether.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;
using static Ether.Redux.Blazor.ComponentConnector;

namespace Ether.Components.Settings
{
    public class JobLogsProps
    {
        public IEnumerable<JobLogViewModel> Items { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public EventCallback<JobLogViewModel> OnDetailsRequested { get; set; }

        public EventCallback OnRefresh { get; set; }

        public EventCallback OnNextPage { get; set; }

        public EventCallback OnPreviousPage { get; set; }
    }

    public class JobLogsConnected
    {
        private const int ItemsPerPage = 10;

        public static RenderFragment Get()
        {
            var instance = new JobLogsConnected();
            return Connect<JobLogs, RootState, JobLogsProps>(instance.MapStateToProps, instance.MapDispatchToProps, instance.Init);
        }

        private async Task Init(IStore<RootState> store)
        {
            if (!IsJobLogsStateInitialized(store.State))
            {
                await store.Dispatch<FetchJobLogs, FetchJobLogsCommand>(new FetchJobLogsCommand());
            }
        }

        private void MapStateToProps(RootState state, JobLogsProps props)
        {
            var jobLogsState = state?.JobLogs;
            if (jobLogsState != null)
            {
                props.CurrentPage = jobLogsState.CurrentPage;
                props.TotalPages = jobLogsState.TotalPages;
                props.Items = jobLogsState.Items;
            }
        }

        private void MapDispatchToProps(IStore<RootState> store, JobLogsProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnNextPage = EventCallback.Factory.Create(this, () => HandleNextPage(store));
            props.OnPreviousPage = EventCallback.Factory.Create(this, () => HandlePreviousPage(store));
            props.OnDetailsRequested = EventCallback.Factory.Create<JobLogViewModel>(this, l => HandleDetailsRequest(store, l));
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            if (store.State?.JobLogs != null)
            {
                store.Dispatch(new ClearJobLogs());
            }

            await store.Dispatch<FetchJobLogs, FetchJobLogsCommand>(new FetchJobLogsCommand(1));
        }

        private async Task HandleNextPage(IStore<RootState> store)
        {
            var state = store.State.JobLogs;
            if (state.CurrentPage + 1 > state.TotalPages)
            {
                return;
            }

            var needToFetch = state.Items.Count() < (state.CurrentPage + 1) * ItemsPerPage;
            if (needToFetch)
            {
                await store.Dispatch<FetchJobLogs, FetchJobLogsCommand>(new FetchJobLogsCommand(state.CurrentPage + 1));
            }
            else
            {
                store.Dispatch(new JobLogsMoveToPage { CurrentPage = state.CurrentPage + 1});
            }
        }

        private Task HandlePreviousPage(IStore<RootState> store)
        {
            var state = store.State.JobLogs;
            if (state.CurrentPage - 1 == 0)
            {
                return Task.CompletedTask;
            }

            store.Dispatch(new JobLogsMoveToPage { CurrentPage = state.CurrentPage - 1 });
            return Task.CompletedTask;
        }

        private async Task HandleDetailsRequest(IStore<RootState> store, JobLogViewModel log)
        {
            if (log.Details == null)
            {
                await store.Dispatch<FetchJobDetailsAction, JobLogViewModel>(log);
            }
        }

        private bool IsJobLogsStateInitialized(RootState state)
        {
            return state?.JobLogs != null;
        }
    }
}

using System;
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
    public class JobLogsConnected
    {
        private const int ItemsPerPage = 10;

        public static RenderFragment Get()
        {
            var instance = new JobLogsConnected();
            return Connect<JobLogs, RootState, JobLogsProps>(instance.MapStateToProps, instance.MapDispatchToProps);
        }

        public void MapStateToProps(RootState state, JobLogsProps props)
        {
            var jobLogsState = state.JobLogs;
            Console.WriteLine($"Mapping state. CurrentPage: {jobLogsState.CurrentPage}; Items: {jobLogsState.Items?.Count()}");

            props.CurrentPage = jobLogsState.CurrentPage;
            props.TotalPages = jobLogsState.TotalPages;
            props.Items = jobLogsState.Items;
        }

        public void MapDispatchToProps(IStore<RootState> store, JobLogsProps props)
        {
            props.OnRefresh = EventCallback.Factory.Create(this, () => HandleRefresh(store));
            props.OnNextPage = EventCallback.Factory.Create(this, () => HandleNextPage(store));
            props.OnPreviousPage = EventCallback.Factory.Create(this, () => HandlePreviousPage(store));
            props.OnDetailsRequested = HandleDetailsRequest;
        }

        private async Task HandleRefresh(IStore<RootState> store)
        {
            if (store.State?.JobLogs?.CurrentPage != 0)
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

        private async Task<Ether.ViewModels.Types.JobDetails> HandleDetailsRequest(JobLogViewModel log)
        {
            throw new Exception();
        }
    }
}

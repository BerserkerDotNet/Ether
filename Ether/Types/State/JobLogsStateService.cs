using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.ViewModels.Types;

namespace Ether.Types.State
{
    public class JobLogsStateService
    {
        private const int ItemsPerPage = 10;
        private readonly EtherClient _client;
        private readonly AppState _state;

        public JobLogsStateService(EtherClient client, AppState state)
        {
            _client = client;
            _state = state;
        }

        public JobLogsState State => _state.JobLogs;

        public IEnumerable<JobLogViewModel> Logs => State?.Items;

        public int CurrentPage => State?.CurrentPage ?? 0;

        public int TotalPages => State?.TotalPages ?? 0;

        public async Task LoadAsync(bool hard = false)
        {
            if (hard)
            {
                _state.JobLogs = null;
            }

            if (_state.JobLogs == null)
            {
                var page = await _client.GetAllPaged<JobLogViewModel>();
                _state.JobLogs = new JobLogsState(page.Items, page.CurrentPage, page.TotalPages);
            }
        }

        public async Task GoToNextPage()
        {
            if (State == null)
            {
                throw new ArgumentException("State is not initialized. Call 'Load' before going to next page.");
            }

            if (State.CurrentPage + 1 > State.TotalPages)
            {
                return;
            }

            var needToFetch = State.Items.Count() < (State.CurrentPage + 1) * ItemsPerPage;
            if (needToFetch)
            {
                var page = await _client.GetAllPaged<JobLogViewModel>(page: State.CurrentPage + 1, itemsPerPage: ItemsPerPage);
                var allItems = State.Items.Union(page.Items).ToArray();
                _state.JobLogs = new JobLogsState(allItems, page.CurrentPage, page.TotalPages);
            }
            else
            {
                _state.JobLogs = new JobLogsState(State.Items, State.CurrentPage + 1, State.TotalPages);
            }
        }

        public Task GoToPreviousPage()
        {
            if (State == null)
            {
                throw new ArgumentException("State is not initialized. Call 'Load' before going to prev page.");
            }

            if (State.CurrentPage - 1 == 0)
            {
                return Task.CompletedTask;
            }

            _state.JobLogs = new JobLogsState(State.Items, State.CurrentPage - 1, State.TotalPages);
            Console.WriteLine($"Current page: {State.CurrentPage}");

            return Task.CompletedTask;
        }

        public async Task<JobDetails> GetJobDetails(JobLogViewModel log)
        {
            if (log.JobType == "PullRequestsSyncJob")
            {
                return await _client.GetJobDetailsById<PullRequestJobDetails>(log.Id);
            }

            return null;
        }
    }
}

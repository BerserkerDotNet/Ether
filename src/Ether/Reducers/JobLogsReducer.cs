using System.Linq;
using Ether.Actions;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;
using Ether.ViewModels;

namespace Ether.Reducers
{
    public class JobLogsReducer : IReducer<JobLogsState>
    {
        public JobLogsState Reduce(JobLogsState state, IAction action)
        {
            switch (action)
            {
                case ReceivedJobLogsPage a:
                    var currentItems = state?.Items ?? Enumerable.Empty<JobLogViewModel>();
                    var newItems = currentItems.Union(a.Logs).ToArray();
                    return new JobLogsState(newItems, a.CurrentPage, a.TotalPages);
                case ClearJobLogs a:
                    return new JobLogsState(Enumerable.Empty<JobLogViewModel>(), 0, 0);
                case JobLogsMoveToPage a:
                    return new JobLogsState(state.Items, a.CurrentPage, state.TotalPages);
                case UpdateJobLogDetail a:
                    var items = state.Items;
                    var itemToModify = items.Single(i => i.Id == a.JobLogId);
                    itemToModify.Details = a.Details;
                    return new JobLogsState(items, state.CurrentPage, state.TotalPages);
                default:
                    return state;
            }
        }
    }
}

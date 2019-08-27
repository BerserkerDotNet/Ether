using Ether.Actions;
using Ether.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class LoadJobLogsReducer : IReducer<JobLogsState>
    {
        public JobLogsState Reduce(JobLogsState state, IAction action)
        {
            switch (action)
            {
                case LoadJobLogs a:
                    return new JobLogsState(a.Logs, 1, a.TotalPages);
                default:
                    return state;
            }
        }
    }
}

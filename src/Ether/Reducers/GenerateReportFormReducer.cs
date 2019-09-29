using Ether.Actions;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class GenerateReportFormReducer : IReducer<GenerateReportFormState>
    {
        public GenerateReportFormState Reduce(GenerateReportFormState state, IAction action)
        {
            switch (action)
            {
                case ReceiveReportDescriptorsAction a:
                    return new GenerateReportFormState(state?.Form, a.ReportDescriptors);
                case ReceiveReportRequestAction a:
                    return new GenerateReportFormState(a.Request, state?.ReportTypes);
                default:
                    return state;
            }
        }
    }
}

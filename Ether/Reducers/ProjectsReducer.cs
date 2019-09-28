using Ether.Actions;
using BlazorState.Redux.Interfaces;
using Ether.Types.State;

namespace Ether.Reducers
{
    public class ProjectsReducer : IReducer<ProjectsState>
    {
        public ProjectsState Reduce(ProjectsState state, IAction action)
        {
            switch (action)
            {
                case ReceiveProjectsAction a:
                    return new ProjectsState(a.Projects);
                default:
                    return state;
            }
        }
    }
}

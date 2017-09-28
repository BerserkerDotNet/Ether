using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using System;
using System.Linq;
using Ether.Core.Models.VSTS;
using System.Collections.Generic;
using Ether.Core.Models.DTO;

namespace Ether.Core.Reporters.Classifiers
{
    public class ResolvedWorkItemsClassifier : BaseWorkItemsClassifier
    {
        public ResolvedWorkItemsClassifier()
            :base(WorkItemTypes.Bug, WorkItemTypes.Task)
        {

        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => u.State.NewValue == ResolvedState 
                    && u.State.OldValue != ClosedState
                    && request.Team.Any(t => !string.IsNullOrEmpty(u.ResolvedBy.NewValue) && u.ResolvedBy.NewValue.Contains(t.Email)));
            if (resolutionUpdate == null)
                return WorkItemResolution.None;

            var resolvedByMemeber = request.Team.Single(m => resolutionUpdate.ResolvedBy.NewValue.Contains(m.Email));
            return new WorkItemResolution(request.WorkItem, ResolvedState, resolutionUpdate.Reason.NewValue,
                resolutionUpdate.RevisedDate, resolvedByMemeber.Email, resolvedByMemeber.DisplayName);
        }
    }

    public class ClosedTasksWorkItemsClassifier : BaseWorkItemsClassifier
    {
        public ClosedTasksWorkItemsClassifier()
            : base(WorkItemTypes.Task)
        {

        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            var resolutionUpdate = request.WorkItem.Updates.LastOrDefault(u => WasClosedByTeamMember(u, request.Team));
            var wasEverResolved = request.WorkItem.Updates.Any(u => u.State.NewValue == ResolvedState);
            if (resolutionUpdate == null || wasEverResolved)
                return WorkItemResolution.None;

            var reason = resolutionUpdate.Reason.NewValue;
            var closedByMemeber = request.Team.Single(m => resolutionUpdate.ClosedBy.NewValue.Contains(m.Email));
            return new WorkItemResolution(request.WorkItem, ClosedState, reason, resolutionUpdate.RevisedDate, closedByMemeber.Email, closedByMemeber.DisplayName);
        }

        private bool WasClosedByTeamMember(WorkItemUpdate update, IEnumerable<TeamMember> team)
        {
            var closedBy = update.ClosedBy.NewValue;
            return update.State.NewValue == ClosedState
                && update.State.OldValue != ResolvedState
                && team.Any(t => !string.IsNullOrEmpty(closedBy) && closedBy.Contains(t.Email));
        }
    }

    public class InvestigatedWorkItemsClassifier : IWorkItemsClassifier
    {
        public WorkItemResolution Classify(WorkItemResolutionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

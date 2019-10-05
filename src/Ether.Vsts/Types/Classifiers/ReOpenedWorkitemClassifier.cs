using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Types.Classifiers
{
    public class ReOpenedWorkitemClassifier : VstsBaseWorkItemsClassifier
    {
        private readonly ILogger<ReOpenedWorkitemClassifier> _logger;

        public ReOpenedWorkitemClassifier(ILogger<ReOpenedWorkitemClassifier> logger)
        {
            _logger = logger;
        }

        protected override IEnumerable<IWorkItemEvent> ClassifyInternal(WorkItemResolutionRequest request)
        {
            var wrapper = GetWorkItemWrapper(request.WorkItem);
            var updates = wrapper.Updates;

            var reOpens = updates.Where(u => (IsResolved(u.State.Old) || IsClosed(u.State.Old)) && !IsClosed(u.State.New));
            foreach (var reOpen in reOpens)
            {
                // TODO: Consider using update id instead of relying on reference equality
                var previousUpdates = updates.TakeWhile(u => u != reOpen);
                var resolvedUpdate = previousUpdates.LastOrDefault(u => IsResolved(u.State.New));
                if (resolvedUpdate is null)
                {
                    resolvedUpdate = previousUpdates.LastOrDefault(u => IsClosed(u.State.New));
                }

                if (resolvedUpdate is null)
                {
                    _logger.LogWarning("Encountered ReOpenEvent without corresponding Resolved or Closed update while classifying {WorkItem}.", wrapper.Id);
                    continue;
                }

                var associatedUser = IsOnTheTeam(resolvedUpdate.AssignedTo.Old, request.Team) ? resolvedUpdate.AssignedTo.Old : null;
                if (associatedUser is null)
                {
                    if (IsResolved(resolvedUpdate.State.New))
                    {
                        associatedUser = IsOnTheTeam(resolvedUpdate.ResolvedBy.New, request.Team) ? resolvedUpdate.ResolvedBy.New : null;
                    }
                    else if (IsClosed(resolvedUpdate.State.New))
                    {
                        associatedUser = IsOnTheTeam(resolvedUpdate.ClosedBy.New, request.Team) ? resolvedUpdate.ClosedBy.New : null;
                    }
                }

                if (associatedUser is null)
                {
                    _logger.LogInformation("Reopen detected for Work item '{WorkItem}' but it was resolved not by team member.", wrapper.Id);
                    continue;
                }

                yield return new WorkItemReOpenedEvent(wrapper, reOpen.ChangedDate.New, associatedUser);
            }
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            var wrapper = GetWorkItemWrapper(item);
            return string.Equals(wrapper.Type, Constants.WorkItemTypeBug, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool IsOnTheTeam(UserReference member, IEnumerable<TeamMemberViewModel> team)
        {
            if (member is null || team is null || !team.Any())
            {
                return false;
            }

            return team.Any(m => string.Equals(m.Email, member.Email, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsResolved(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsClosed(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateClosed);
        }

        private bool IsActive(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsNew(string state)
        {
            return IsStateEqual(state, Constants.WorkItemStateResolved);
        }

        private bool IsStateEqual(string state, string expectedState)
        {
            return string.Equals(state, expectedState, StringComparison.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Core.Constants;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;
using Ether.Core.Types.Exceptions;

namespace Ether.Core.Utils
{
    public static class WorkItemUtils
    {
        public static float GetEtaValue(this VSTSWorkItem wi, ETAFieldType etaType, Settings settings)
        {
            var fieldName = FieldNameFor(wi.WorkItemType, etaType, settings);
            if (!wi.Fields.ContainsKey(fieldName))
                return etaType == ETAFieldType.RemainingWork ? TryGetFromUpdates(wi, fieldName) : 0;

            var value = wi.Fields[fieldName];
            if (string.IsNullOrEmpty(value))
                return 0;

            return float.Parse(value);
        }

        private static float TryGetFromUpdates(VSTSWorkItem wi, string fieldName)
        {
            var recoveredValue = wi.Updates
                .LastOrDefault(u => (u.State?.NewValue == WorkItemStates.Closed || u.State?.NewValue == WorkItemStates.Resolved) && u.Fields.ContainsKey(fieldName))?
                .Fields[fieldName].OldValue;

            float.TryParse(recoveredValue, out var result);
            return result;
        }

        public static string FieldNameFor(string workItemType, ETAFieldType fieldType, Settings settings)
        {
            var etaFields = settings?.WorkItemsSettings?.ETAFields;
            if (etaFields == null || !etaFields.Any())
                throw new MissingETASettingsException();

            return etaFields.First(f => f.WorkitemType == workItemType && f.FieldType == fieldType).FieldName;
        }

        public static float GetActiveDuration(this VSTSWorkItem workItem, IEnumerable<TeamMember> team)
        {
            if (workItem.Updates == null || !workItem.Updates.Any())
                return 0.0f;

            var activeTime = 0.0F;
            var isActive = false;
            var assignedToTeam = false;
            DateTime? lastActivated = null;
            foreach (var update in workItem.Updates)
            {
                var isActivation = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.Active;
                var isOnHold = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.New;
                var isResolved = !update.State.IsEmpty && (update.State.NewValue == WorkItemStates.Resolved || update.State.NewValue == WorkItemStates.Closed);
                var isCodeReview = !update.Tags.IsEmpty && WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.CodeReview) || update.Relations?.Added != null
                                   && update.Relations.Added.Any(i => !string.IsNullOrWhiteSpace(i.Name) && i.Name.Equals("Pull Request", StringComparison.InvariantCultureIgnoreCase));
                var isBlocked = !update.Tags.IsEmpty &&
                    (WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.Blocked) || WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.OnHold));
                var isUnBlocked = !isBlocked && !update.Tags.IsEmpty &&
                    (WorkItemTags.ContainsTag(update.Tags.OldValue, WorkItemTags.Blocked) && !WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.Blocked) ||
                     WorkItemTags.ContainsTag(update.Tags.OldValue, WorkItemTags.OnHold) && !WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.OnHold));

                if (!assignedToTeam && !string.IsNullOrWhiteSpace(update.AssignedTo.NewValue))
                {
                    assignedToTeam = team.Any(m => update.AssignedTo.NewValue.Contains(m.Email));
                    if (isActive) lastActivated = update.ChangedDate;
                }

                if (isActive && (isOnHold || isBlocked))
                {
                    isActive = false;
                    if (lastActivated != null && assignedToTeam)
                        activeTime += lastActivated.Value.CountBusinessDaysThrough(update.ChangedDate);
                }
                else if ((isActivation && !isBlocked) || isUnBlocked)
                {
                    lastActivated = update.ChangedDate;
                    isActive = true;
                }

                else if (isResolved || isCodeReview)
                {
                    if (isActive && lastActivated != null && assignedToTeam)
                        activeTime += lastActivated.Value.CountBusinessDaysThrough(update.ChangedDate);
                    break;
                }
            }

            return activeTime;
        }

        public static bool IsInCodeReview(this VSTSWorkItem item)
        {
            return item.State == WorkItemStates.Active
                   && (WorkItemTags.ContainsTag(item.Updates.LastOrDefault(u => !u.Tags.IsEmpty)?.Tags.NewValue, WorkItemTags.CodeReview)
                       || item.Updates.Any(u => u.Relations?.Added != null && u.Relations.Added.Any(i => i.IsPullRequest)));
        }

        public static bool IsAssignedToTeamMember(this VSTSWorkItem item, IEnumerable<TeamMember> team)
        {
            var assignedTo = item.Updates.LastOrDefault(u => !u.AssignedTo.IsEmpty)?.AssignedTo.NewValue;
            return team.Any(m => !string.IsNullOrWhiteSpace(assignedTo) && assignedTo.Contains(m.Email));
        }
    }
}
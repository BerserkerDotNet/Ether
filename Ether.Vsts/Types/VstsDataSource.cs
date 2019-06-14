using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Core.Extensions;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Dto;
using Ether.Vsts.Interfaces;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types
{
    public class VstsDataSource : IDataSource
    {
        private const string ArtifactLink = "ArtifactLink";
        private const string PullRequestId = "PullRequestId";
        private const string ForwardSlash = "/";

        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public VstsDataSource(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public VstsDataSource()
        {
        }

        public async Task<ProfileViewModel> GetProfile(Guid id)
        {
            var profile = await _repository.GetSingleAsync<VstsProfile>(id);
            return _mapper.Map<ProfileViewModel>(profile);
        }

        public async Task<TeamMemberViewModel> GetTeamMember(Guid id)
        {
            var member = await _repository.GetSingleAsync<TeamMember>(id);
            return _mapper.Map<TeamMemberViewModel>(member);
        }

        public async Task<IEnumerable<PullRequestViewModel>> GetPullRequests(Expression<Func<PullRequestViewModel, bool>> predicate)
        {
            var pullRequests = await _repository.GetAllAsync<PullRequest>();
            var result = pullRequests
                .Select(p => _mapper.Map<PullRequestViewModel>(p))
                .Where(predicate.Compile())
                .ToArray();

            return result;
        }

        public ETAValues GetETAValues(WorkItemViewModel workItem)
        {
            return new ETAValues(
                GetEtaValue(workItem, OriginalEstimateField),
                GetEtaValue(workItem, RemainingWorkField),
                GetEtaValue(workItem, CompletedWorkField));
        }

        public async Task<IEnumerable<WorkItemViewModel>> GetWorkItemsFor(Guid memberId)
        {
            var relatedWorkItems = await _repository.GetFieldValueAsync<TeamMember, int[]>(memberId, m => m.RelatedWorkItems);
            if (relatedWorkItems == null || !relatedWorkItems.Any())
            {
                return Enumerable.Empty<WorkItemViewModel>();
            }

            var workitems = await _repository.GetAsync<WorkItem>(w => relatedWorkItems.Contains(w.WorkItemId));
            return _mapper.MapCollection<WorkItemViewModel>(workitems);
        }

        public float GetActiveDuration(WorkItemViewModel workItem)
        {
            if (workItem.Updates == null || !workItem.Updates.Any())
            {
                return 0.0f;
            }

            var activeTime = 0.0F;
            var isActive = false;
            DateTime? lastActivated = null;
            foreach (var update in workItem.Updates)
            {
                var isActivation = !update[WorkItemStateField].IsEmpty() && update[WorkItemStateField].NewValue == WorkItemStateActive;
                var isOnHold = !update[WorkItemStateField].IsEmpty() && update[WorkItemStateField].NewValue == WorkItemStateNew;
                var isResolved = !update[WorkItemStateField].IsEmpty() && (update[WorkItemStateField].NewValue == WorkItemStateResolved || update[WorkItemStateField].NewValue == WorkItemStateClosed);
                var isCodeReview = !update[WorkItemTagsField].IsEmpty() && ContainsTag(update[WorkItemTagsField].NewValue, CodeReviewTag);
                var isBlocked = !update[WorkItemTagsField].IsEmpty() &&
                    (ContainsTag(update[WorkItemTagsField].NewValue, BlockedTag) || ContainsTag(update[WorkItemTagsField].NewValue, OnHoldTag));
                var isUnBlocked = !isBlocked && !update[WorkItemTagsField].IsEmpty() &&
                    (ContainsTag(update[WorkItemTagsField].OldValue, BlockedTag) && !ContainsTag(update[WorkItemTagsField].NewValue, BlockedTag) ||
                    ContainsTag(update[WorkItemTagsField].OldValue, OnHoldTag) && !ContainsTag(update[WorkItemTagsField].NewValue, OnHoldTag));

                if (isActive && (isOnHold || isBlocked))
                {
                    isActive = false;
                    if (lastActivated != null)
                    {
                        activeTime += CountBusinessDaysBetween(lastActivated.Value, DateTime.Parse(update[WorkItemChangedDateField].NewValue));
                    }
                }
                else if ((isActivation && !isBlocked) || isUnBlocked)
                {
                    lastActivated = DateTime.Parse(update[WorkItemChangedDateField].NewValue);
                    isActive = true;
                }
                else if (isActive && (isResolved || isCodeReview))
                {
                    if (lastActivated != null)
                    {
                        activeTime += CountBusinessDaysBetween(lastActivated.Value, DateTime.Parse(update[WorkItemChangedDateField].NewValue));
                    }

                    break;
                }
            }

            return activeTime;
        }

        public async Task<bool> IsInCodeReview(WorkItemViewModel workItem)
        {
            var isActiveWithCodeReviewTag = IsActive(workItem) && workItem.Fields.ContainsKey(WorkItemTagsField) && ContainsTag(workItem.Fields[WorkItemTagsField], CodeReviewTag);
            if (isActiveWithCodeReviewTag)
            {
                return true;
            }

            if (!IsActive(workItem))
            {
                return false;
            }

            var pullRequestIds = workItem.Relations?
                .Where(r => string.Equals(r.RelationType, ArtifactLink, StringComparison.OrdinalIgnoreCase) && r.Url.LocalPath.Contains(PullRequestId))
                .Select(r => r.Url.LocalPath.Substring(r.Url.LocalPath.LastIndexOf(ForwardSlash) + 1))
                .Select(id => int.Parse(id))
                .ToArray();

            if (pullRequestIds == null || !pullRequestIds.Any())
            {
                return false;
            }

            var pullRequests = await _repository.GetAsync<PullRequest>(p => pullRequestIds.Contains(p.PullRequestId));
            return pullRequests.Count() == pullRequestIds.Count() && pullRequests.Any(p => p.State == PullRequestState.Active);
        }

        public bool IsActive(WorkItemViewModel workItem)
        {
            return string.Equals(workItem[WorkItemStateField], WorkItemStateActive, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsResolved(IEnumerable<WorkItemResolution> resolutions)
        {
            return resolutions.Any(r => r.Resolution == WorkItemStateResolved || (r.WorkItemType == WorkItemTypeTask && r.Resolution == WorkItemStateClosed));
        }

        public bool IsAssignedToTeamMember(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team)
        {
            var assignedTo = workItem.Updates.LastOrDefault(u => !u[WorkItemAssignedToField].IsEmpty())?[WorkItemAssignedToField].NewValue;
            return team.Any(m => !string.IsNullOrWhiteSpace(assignedTo) && assignedTo.Contains(m.Email));
        }

        // TODO: DataSource should not have knowledge on the type specific to reporters!
        public WorkItemDetail CreateWorkItemDetail(WorkItemViewModel item)
        {
            var timeSpent = GetActiveDuration(item);
            var etaValues = GetETAValues(item);

            return new WorkItemDetail
            {
                WorkItemId = item.WorkItemId,
                WorkItemTitle = item[WorkItemTitleField],
                WorkItemType = item[WorkItemTypeField],
                WorkItemProject = string.IsNullOrWhiteSpace(item[WorkItemAreaPathField]) ? null : item[WorkItemAreaPathField].Split('\\')[0],
                Tags = item.Updates.LastOrDefault(u => !string.IsNullOrWhiteSpace(u[WorkItemTagsField].NewValue))?[WorkItemTagsField]?.NewValue,
                OriginalEstimate = etaValues.OriginalEstimate,
                EstimatedToComplete = etaValues.RemainingWork + etaValues.CompletedWork,
                TimeSpent = timeSpent
            };
        }

        private bool ContainsTag(string tags, string tag)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return false;
            }

            return tags.Split(';')
                .Select(t => t.Replace(" ", string.Empty).ToLower())
                .Contains(tag);
        }

        private float GetEtaValue(WorkItemViewModel wi, string field)
        {
            if (!wi.Fields.ContainsKey(field))
            {
                return GetEtaFromUpdates(wi, field);
            }

            var value = wi[field];
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return float.Parse(value);
        }

        private float GetEtaFromUpdates(WorkItemViewModel wi, string field)
        {
            if (wi.Updates == null || !wi.Updates.Any())
            {
                return 0;
            }

            var recoveredValue = wi.Updates
                .LastOrDefault(u => (u[WorkItemStateField]?.NewValue == WorkItemStateClosed || u[WorkItemStateField]?.NewValue == WorkItemStateResolved) && u.Fields.ContainsKey(field))?
                .Fields[field].OldValue;

            float.TryParse(recoveredValue, out var result);
            return result;
        }

        private float CountBusinessDaysBetween(DateTime firstDay, DateTime lastDay, params DateTime[] holidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
            {
                throw new ArgumentException("Incorrect last day " + lastDay);
            }

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days;
            if (businessDays == 0)
            {
                return 1;
            }

            int fullWeekCount = businessDays / 7;

            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                {
                    lastDayOfWeek += 7;
                }

                if (firstDayOfWeek <= 6)
                {
                    // Both Saturday and Sunday are in the remaining time interval
                    if (lastDayOfWeek >= 7)
                    {
                        businessDays -= 2;
                    }

                    // Only Saturday is in the remaining time interval
                    else if (lastDayOfWeek >= 6)
                    {
                        businessDays -= 1;
                    }
                }

                // Only Sunday is in the remaining time interval
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)
                {
                    businessDays -= 1;
                }
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (var holiday in holidays)
            {
                var bh = holiday.Date;
                if (firstDay <= bh && bh <= lastDay)
                {
                    --businessDays;
                }
            }

            return businessDays;
        }
    }
}
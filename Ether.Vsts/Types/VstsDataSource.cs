using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Core.Extensions;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types
{
    public class VstsDataSource : IDataSource
    {
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

        public Task<ETAValues> GetETAValues(WorkItemViewModel workItem)
        {
            return Task.FromResult(new ETAValues(
                GetEtaValue(workItem, Constants.OriginalEstimateField),
                GetEtaValue(workItem, Constants.RemainingWorkField),
                GetEtaValue(workItem, Constants.CompletedWorkField)));
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
            var value = wi[field];
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return float.Parse(value);
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
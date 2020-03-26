using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Core.Extensions;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Dto;
using Ether.Vsts.Interfaces;
using Microsoft.Extensions.Logging;
using static Ether.Contracts.Types.WorkdaysAmountUtil;
using static Ether.Vsts.Constants;

namespace Ether.Vsts.Types
{
    public class VstsDataSource : IDataSource
    {
        private const string ArtifactLink = "ArtifactLink";
        private const string PullRequestId = "PullRequestId";
        private const string ForwardSlash = "/";
        private const float MinimumPossibleEstimate = 1.0f;

        private static readonly Regex _userInfoParser = new Regex("(?<Name>[\\w\\d\\s]+)\\s(?<Company>\\([\\w\\d\\s]+\\))?\\s?\\<(?<Email>[^>]+)\\>", RegexOptions.Compiled);

        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<VstsDataSource> _logger;
        private Lazy<VstsDataSourceSettings> _vstsConfigCache;

        public VstsDataSource(IRepository repository, IMapper mapper, ILogger<VstsDataSource> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _vstsConfigCache = new Lazy<VstsDataSourceSettings>(() => repository.GetSingle<VstsDataSourceSettings>(_ => true));
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

        public float GetActiveDuration(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team)
        {
            if (workItem.Updates == null || !workItem.Updates.Any())
            {
                return MinimumPossibleEstimate;
            }

            var activeTime = 0.0F;
            var isActive = false;
            var assignedToTeam = false;
            DateTime? lastActivated = null;
            foreach (var update in workItem.Updates)
            {
                var isActivation = !update[WorkItemStateField].IsEmpty() && update[WorkItemStateField].NewValue == WorkItemStateActive;
                var isOnHold = !update[WorkItemStateField].IsEmpty() && update[WorkItemStateField].NewValue == WorkItemStateNew;
                var isResolved = !update[WorkItemStateField].IsEmpty() && (update[WorkItemStateField].NewValue == WorkItemStateResolved || update[WorkItemStateField].NewValue == WorkItemStateClosed);
                var isCodeReview = !update[WorkItemTagsField].IsEmpty() && ContainsTag(update[WorkItemTagsField].NewValue, CodeReviewTag) ||
                    (update.Relations?.Added != null && update.Relations.Added.Any(i => i.Attributes.ContainsKey("name") && !string.IsNullOrWhiteSpace(i.Attributes["name"]) && i.Attributes["name"].Equals("Pull Request", StringComparison.InvariantCultureIgnoreCase)));
                var isBlocked = !update[WorkItemTagsField].IsEmpty() &&
                    (ContainsTag(update[WorkItemTagsField].NewValue, BlockedTag) || ContainsTag(update[WorkItemTagsField].NewValue, OnHoldTag));
                var isUnBlocked = !isBlocked && !update[WorkItemTagsField].IsEmpty() &&
                    (ContainsTag(update[WorkItemTagsField].OldValue, BlockedTag) && !ContainsTag(update[WorkItemTagsField].NewValue, BlockedTag) ||
                    ContainsTag(update[WorkItemTagsField].OldValue, OnHoldTag) && !ContainsTag(update[WorkItemTagsField].NewValue, OnHoldTag));

                var isInCodeReviewOrResolved = isResolved || isCodeReview;

                if (!assignedToTeam && !string.IsNullOrWhiteSpace(update[WorkItemAssignedToField].NewValue))
                {
                    assignedToTeam = team.Any(m => update[WorkItemAssignedToField].NewValue.Contains(m.Email));
                    if (isActive)
                    {
                        lastActivated = DateTime.Parse(update[WorkItemChangedDateField].NewValue);
                    }
                }

                if (isActive && (isOnHold || isBlocked))
                {
                    isActive = false;
                    if (lastActivated != null && assignedToTeam)
                    {
                        activeTime += CalculateWorkdaysAmount(lastActivated.Value, DateTime.Parse(update[WorkItemChangedDateField].NewValue));
                    }
                }
                else if ((isActivation && !isBlocked) || (isUnBlocked && !isInCodeReviewOrResolved))
                {
                    lastActivated = DateTime.Parse(update[WorkItemChangedDateField].NewValue);
                    isActive = true;
                }
                else if (isInCodeReviewOrResolved)
                {
                    if (isActive && lastActivated != null && assignedToTeam)
                    {
                        activeTime += CalculateWorkdaysAmount(lastActivated.Value, DateTime.Parse(update[WorkItemChangedDateField].NewValue));
                    }

                    // Prevent short circuiting when pull request is added before the work item activated by team member.
                    if (lastActivated != null && assignedToTeam)
                    {
                        break;
                    }
                }
            }

            return activeTime == 0.0F ? MinimumPossibleEstimate : activeTime;
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

            var pullRequestIds = GetPullRequestIds(workItem);
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

        public bool IsNew(WorkItemViewModel workItem)
        {
            return string.Equals(workItem[WorkItemStateField], WorkItemStateNew, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsResolved(WorkItemViewModel workItem)
        {
            return string.Equals(workItem[WorkItemStateField], WorkItemStateResolved, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(workItem[WorkItemStateField], WorkItemStateClosed, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsResolved(IEnumerable<IWorkItemEvent> resolutions)
        {
            return resolutions.Any(r => r is WorkItemResolvedEvent || (r.WorkItem.Type == WorkItemTypeTask && r is WorkItemClosedEvent));
        }

        public bool IsAssignedToTeamMember(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team)
        {
            var assignedTo = workItem.Updates.LastOrDefault(u => !u[WorkItemAssignedToField].IsEmpty())?[WorkItemAssignedToField].NewValue;
            return team.Any(m => !string.IsNullOrWhiteSpace(assignedTo) && assignedTo.Contains(m.Email));
        }

        // TODO: DataSource should not have knowledge on the type specific to reporters!
        public WorkItemDetail CreateWorkItemDetail(WorkItemViewModel item, IEnumerable<TeamMemberViewModel> team)
        {
            var etaValues = GetETAValues(item);
            (var estimatedToComplete, var timeSpent) = GetEtaMetric(item, team);

            return new WorkItemDetail
            {
                WorkItemId = item.WorkItemId,
                WorkItemTitle = item[WorkItemTitleField],
                WorkItemType = item[WorkItemTypeField],
                WorkItemProject = GetProject(item),
                Tags = item.Updates.LastOrDefault(u => !string.IsNullOrWhiteSpace(u[WorkItemTagsField].NewValue))?[WorkItemTagsField]?.NewValue,
                OriginalEstimate = etaValues.OriginalEstimate,
                EstimatedToComplete = estimatedToComplete,
                TimeSpent = timeSpent,
                Reason = item[WorkItemReasonField]
            };
        }

        // TODO: DataSource should not have knowledge on the type specific to reporters!
        public async Task<WorkitemInformationViewModel> GetWorkItemInfo(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team)
        {
            (var estimatedToComplete, var timeSpent) = GetEtaMetric(workItem, team, allowZeroEstimate: true);
            var isInCodeReview = await IsInCodeReview(workItem);

            if (IsResolved(workItem) || (!IsActive(workItem) && !IsNew(workItem)) || !IsAssignedToTeamMember(workItem, team))
            {
                return null;
            }

            var workItemInfo = new WorkitemInformationViewModel
            {
                Id = workItem.WorkItemId,
                Title = workItem.Fields[WorkItemTitleField],
                State = workItem.Fields[WorkItemStateField],
                Url = GetWorkItemUrl(GetProject(workItem), workItem.WorkItemId),

                // Priority = workItem.Fields[prio]
                AssignedTo = GetUserReference(workItem.Fields[WorkItemAssignedToField]),
                Type = workItem.Fields[WorkItemTypeField],
                IsBlocked = ContainsTag(workItem, "blocked"),
                IsOnHold = ContainsTag(workItem, "onhold"),
                Estimated = estimatedToComplete,
                Spent = timeSpent,
                PullRequests = Enumerable.Empty<WorkitemPullRequest>(),
            };

            var pullRequestIds = GetPullRequestIds(workItem);
            if (pullRequestIds != null && pullRequestIds.Any())
            {
                var pullRequests = await _repository.GetAsync<PullRequest>(p => pullRequestIds.Contains(p.PullRequestId));
                workItemInfo.PullRequests = pullRequestIds.Select(pId =>
                {
                    var prInfo = new WorkitemPullRequest { Id = pId };
                    var pr = pullRequests.SingleOrDefault(p => p.PullRequestId == pId);
                    if (pr == null)
                    {
                        return prInfo;
                    }

                    prInfo.Url = GetPullRequestUrl(GetProject(workItem), pr.Repository, pId);
                    prInfo.Title = pr.Title;
                    prInfo.TimeActive = GetPullRequestActiveTime(pr);
                    prInfo.Author = pr.Author;
                    prInfo.State = pr.State.ToString();

                    return prInfo;
                });
            }

            workItemInfo.Warnings = GetWorkitemWarnings(workItemInfo, workItem);

            return workItemInfo;
        }

        // TODO: Shouldn't be here
        public Task<UnAssignedWorkitemsViewModel> GetUnAssignedFromQuery(Guid queryId)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> GetWorkitemWarnings(WorkitemInformationViewModel workItem, WorkItemViewModel workItemVM)
        {
            if (string.Equals(workItem.State, WorkItemStateActive, StringComparison.OrdinalIgnoreCase) && workItem.Estimated == 0)
            {
                yield return "Workitem does not have an estimate";
            }

            if (workItem.Spent / workItem.Estimated > 1.8f)
            {
                yield return "Workitem is delayed";
            }

            var activePrWithNoCodeReview = workItem.PullRequests.Any(p => string.Equals(workItem.State, "Active", StringComparison.OrdinalIgnoreCase)) && ContainsTag(workItemVM, "code review");
            if (activePrWithNoCodeReview)
            {
                yield return "Active Pull Request, but no 'Code Review' tag.";
            }
        }

        private TimeSpan GetPullRequestActiveTime(PullRequest pr)
        {
            var completedDate = pr.State != PullRequestState.Active ? pr.Completed : DateTime.UtcNow;
            return completedDate - pr.Created;
        }

        private IEnumerable<int> GetPullRequestIds(WorkItemViewModel workItem)
        {
            var ids = workItem.Relations?
                .Where(r => string.Equals(r.RelationType, ArtifactLink, StringComparison.OrdinalIgnoreCase) && r.Url.LocalPath.Contains(PullRequestId))
                .Select(r => r.Url.LocalPath.Substring(r.Url.LocalPath.LastIndexOf(ForwardSlash) + 1))
                .ToArray();

            if (ids is null)
            {
                return Enumerable.Empty<int>();
            }

            var convertedIds = new List<int>(ids.Count());
            foreach (var idString in ids)
            {
                if (int.TryParse(idString, out var id))
                {
                    convertedIds.Add(id);
                }
                else
                {
                    _logger.LogError("Error parsing pull request for work item {WorkItemId}; String to parse: {StringToParse}", workItem.WorkItemId, idString);
                }
            }

            return convertedIds;
        }

        private (float eta, float spent) GetEtaMetric(WorkItemViewModel workItem, IEnumerable<TeamMemberViewModel> team, bool allowZeroEstimate = false)
        {
            var timeSpent = GetActiveDuration(workItem, team);
            var etaValues = GetETAValues(workItem);

            var estimatedToComplete = etaValues.RemainingWork + etaValues.CompletedWork;
            if (estimatedToComplete == 0)
            {
                estimatedToComplete = etaValues.OriginalEstimate;
            }

            if (estimatedToComplete == 0 && !allowZeroEstimate)
            {
                estimatedToComplete = MinimumPossibleEstimate;
            }

            if (timeSpent == 0 && !allowZeroEstimate)
            {
                timeSpent = MinimumPossibleEstimate;
            }

            return (estimatedToComplete, timeSpent);
        }

        private bool ContainsTag(WorkItemViewModel workItem, string tag)
        {
            if (!workItem.Fields.ContainsKey(WorkItemTagsField))
            {
                return false;
            }

            return ContainsTag(workItem.Fields[WorkItemTagsField], tag);
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

        private UserReference GetUserReference(string user)
        {
            var userRef = new UserReference();
            var match = _userInfoParser.Match(user);
            if (!match.Success)
            {
                return userRef;
            }

            userRef.Title = match.Groups["Name"].Value;
            userRef.Email = match.Groups["Email"].Value;

            return userRef;
        }

        private string GetWorkItemUrl(string project, int workItemId)
        {
            var instance = _vstsConfigCache.Value.InstanceName;
            return $"https://{instance}.visualstudio.com/{project}/_workitems/edit/{workItemId}";
        }

        private string GetPullRequestUrl(string project, Guid repository, int pullRequestId)
        {
            var instance = _vstsConfigCache.Value.InstanceName;
            return $"https://{instance}.visualstudio.com/{project}/_git/{repository}/pullrequest/{pullRequestId}";
        }

        private string GetProject(WorkItemViewModel item)
        {
            return string.IsNullOrWhiteSpace(item[WorkItemAreaPathField]) ? null : item[WorkItemAreaPathField].Split('\\')[0];
        }
    }
}
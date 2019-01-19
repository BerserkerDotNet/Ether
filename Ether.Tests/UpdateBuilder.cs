using System;
using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Tests
{
    public class UpdateBuilder
    {
        public const string DefaultUser = "Foo <foo@bar.com>";
        public const string StateField = "System.State";
        public const string ResolvedByField = "Microsoft.VSTS.Common.ResolvedBy";
        public const string ClosedByField = "Microsoft.VSTS.Common.ClosedBy";
        public const string ReasonField = "System.Reason";
        public const string ChangedDateField = "System.ChangedDate";

        private List<WorkItemUpdateViewModel> _updates = new List<WorkItemUpdateViewModel>();
        private Dictionary<string, WorkItemFieldUpdate> _fields = new Dictionary<string, WorkItemFieldUpdate>();

        public static UpdateBuilder Create()
        {
            return new UpdateBuilder();
        }

        public UpdateBuilder Resolved(TeamMemberViewModel by = null, string from = "Active")
        {
            var resolvedBy = by == null ? DefaultUser : $"{by.DisplayName} <{by.Email}>";
            _fields.Add(StateField, new WorkItemFieldUpdate { NewValue = "Resolved", OldValue = from });
            _fields.Add(ResolvedByField, new WorkItemFieldUpdate { NewValue = resolvedBy });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder Closed(TeamMemberViewModel by = null, string from = "Resolved", string reason = "Fixed")
        {
            var closedBy = by == null ? DefaultUser : $"{by.DisplayName} <{by.Email}>";
            _fields.Add(StateField, new WorkItemFieldUpdate { NewValue = "Closed", OldValue = from });
            _fields.Add(ClosedByField, new WorkItemFieldUpdate { NewValue = closedBy });
            _fields.Add(ReasonField, new WorkItemFieldUpdate { NewValue = reason });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder ClosedFromActive(TeamMemberViewModel by = null, string from = "Active", string reason = "Fixed")
        {
            return Closed(by, from, reason);
        }

        public UpdateBuilder Activated(string from = "New")
        {
            _fields.Add(StateField, new WorkItemFieldUpdate { NewValue = "Active", OldValue = from });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder On(DateTime changedDate)
        {
            _fields[ChangedDateField] = new WorkItemFieldUpdate { NewValue = changedDate.ToString("s") };
            return this;
        }

        public IEnumerable<WorkItemUpdateViewModel> GetActivated(string from = "New")
        {
            return Activated(from).Build();
        }

        public UpdateBuilder New(string from = "")
        {
            _fields.Add(StateField, new WorkItemFieldUpdate { NewValue = "New", OldValue = from });
            On(DateTime.UtcNow);
            return this;
        }

        public IEnumerable<WorkItemUpdateViewModel> GetNew(string from = "")
        {
            return New(from).Build();
        }

        public UpdateBuilder Repathed(string to)
        {
            return new UpdateBuilder();
        }

        public UpdateBuilder Update()
        {
            return new UpdateBuilder();
        }

        public UpdateBuilder Because(string reason)
        {
            _fields.Add(ReasonField, new WorkItemFieldUpdate { NewValue = reason });
            return this;
        }

        public UpdateBuilder With(string fieldName, string newValue, string oldValue)
        {
            return With(fieldName, new WorkItemFieldUpdate { NewValue = newValue, OldValue = oldValue });
        }

        public UpdateBuilder With(string fieldName, WorkItemFieldUpdate value)
        {
            _fields[fieldName] = value;
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder Then()
        {
            AddNewUpdate();
            Reset();
            return this;
        }

        public IEnumerable<WorkItemUpdateViewModel> Build()
        {
            AddNewUpdate();
            return _updates;
        }

        private void AddNewUpdate()
        {
            var wi = new WorkItemUpdateViewModel { Fields = _fields };
            _updates.Add(wi);
        }

        private void Reset()
        {
            _fields = new Dictionary<string, WorkItemFieldUpdate>();
        }
    }
}

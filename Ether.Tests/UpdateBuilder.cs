using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using System;
using System.Collections.Generic;

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

        List<WorkItemUpdate> _updates = new List<WorkItemUpdate>();
        private Dictionary<string, WorkItemUpdate.UpdateValue> _fields = new Dictionary<string, WorkItemUpdate.UpdateValue>();
        private DateTime _revisedDate = DateTime.MinValue;

        public static UpdateBuilder Create()
        {
            return new UpdateBuilder();
        }

        public UpdateBuilder Resolved(TeamMember by = null, string from = "Active")
        {
            var resolvedBy = by == null ? DefaultUser : $"{by.DisplayName} <{by.Email}>";
            _fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "Resolved", OldValue = from });
            _fields.Add(ResolvedByField, new WorkItemUpdate.UpdateValue { NewValue = resolvedBy });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder Closed(TeamMember by = null, string from = "Resolved", string reason = "Fixed")
        {
            var closedBy = by == null ? DefaultUser : $"{by.DisplayName} <{by.Email}>";
            _fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "Closed", OldValue = from });
            _fields.Add(ClosedByField, new WorkItemUpdate.UpdateValue { NewValue = closedBy });
            _fields.Add(ReasonField, new WorkItemUpdate.UpdateValue { NewValue = reason });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder ClosedFromActive(TeamMember by = null, string from = "Active", string reason = "Fixed")
        {
            return Closed(by, from, reason);
        }

        public UpdateBuilder Activated(string from = "New")
        {
            _fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "Active", OldValue = from });
            On(DateTime.UtcNow);
            return this;
        }

        public UpdateBuilder On(DateTime revisedDate, DateTime? changedDate = null)
        {
            if (changedDate == null)
                changedDate = revisedDate;

            _revisedDate = revisedDate;

            _fields[ChangedDateField] = new WorkItemUpdate.UpdateValue { NewValue = changedDate.Value.ToString("s") };
            return this;
        }

        public IEnumerable<WorkItemUpdate> GetActivated(DateTime revisedOn, string from = "New")
        {
            _revisedDate = revisedOn;
            return Activated(from).Build();
        }

        public UpdateBuilder New(string from = "")
        {
            _fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "New", OldValue = from });
            On(DateTime.UtcNow);
            return this;
        }

        public IEnumerable<WorkItemUpdate> GetNew(DateTime revisedOn, string from = "")
        {
            _revisedDate = revisedOn;
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
            _fields.Add(ReasonField, new WorkItemUpdate.UpdateValue { NewValue = reason });
            return this;
        }

        public UpdateBuilder With(string fieldName, string newValue, string oldValue)
        {
            return With(fieldName, new WorkItemUpdate.UpdateValue { NewValue = newValue, OldValue = oldValue });
        }

        public UpdateBuilder With(string fieldName, WorkItemUpdate.UpdateValue value)
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

        public IEnumerable<WorkItemUpdate> Build()
        {
            AddNewUpdate();
            return _updates;
        }


        private void AddNewUpdate()
        {
            var wi = new WorkItemUpdate { Fields = _fields, RevisedDate = _revisedDate };
            _updates.Add(wi);
        }

        private void Reset()
        {
            _revisedDate = DateTime.MinValue;
            _fields = new Dictionary<string, WorkItemUpdate.UpdateValue>();
        }
    }
}

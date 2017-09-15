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
        public const string ReasonField = "System.Reason";

        private Dictionary<string, WorkItemUpdate.UpdateValue> _fields = new Dictionary<string, WorkItemUpdate.UpdateValue>();

        public static ResolvedUpdateBuilder Resolved(TeamMember by = null, string from = "Active")
        {
            var resolvedBy = by == null ? DefaultUser : $"{by.DisplayName} <{by.Email}>";
            var builder = new ResolvedUpdateBuilder();
            builder._fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "Resolved", OldValue = from });
            builder._fields.Add(ResolvedByField, new WorkItemUpdate.UpdateValue { NewValue = resolvedBy });
            return builder;
        }

        public static ActivatedUpdateBuilder Activated(string from = "New")
        {
            var builder = new ActivatedUpdateBuilder();
            builder._fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "Active", OldValue = from });
            return builder;
        }

        public static WorkItemUpdate GetActivated(DateTime revisedOn, string from = "New")
        {
            return Activated(from).Build(revisedOn);
        }

        public static NewUpdateBuilder New(string from = "")
        {
            var builder = new NewUpdateBuilder();
            builder._fields.Add(StateField, new WorkItemUpdate.UpdateValue { NewValue = "New", OldValue = from });
            return builder;
        }

        public static WorkItemUpdate GetNew(DateTime revisedOn, string from = "")
        {
            return New(from).Build(revisedOn);
        }

        public static UpdateBuilder Repathed(string to)
        {
            return new UpdateBuilder();
        }

        public static UpdateBuilder Update()
        {
            return new UpdateBuilder();
        }

        public UpdateBuilder With(string fieldName, string newValue, string oldValue)
        {
            return With(fieldName, new WorkItemUpdate.UpdateValue { NewValue = newValue, OldValue = oldValue });
        }

        public UpdateBuilder With(string fieldName, WorkItemUpdate.UpdateValue value)
        {
            _fields[fieldName] = value;
            return this;
        }

        public WorkItemUpdate Build(DateTime revisedDate)
        {
            return new WorkItemUpdate { Fields = _fields, RevisedDate = revisedDate };
        }

        public WorkItemUpdate Build()
        {
            return Build(DateTime.MinValue);
        }

        public class NewUpdateBuilder : UpdateBuilder
        {

        }

        public class ActivatedUpdateBuilder : UpdateBuilder
        {

        }

        public class ResolvedUpdateBuilder : UpdateBuilder
        {
            public ResolvedUpdateBuilder Because(string reason)
            {
                _fields.Add(ReasonField, new WorkItemUpdate.UpdateValue { NewValue = reason });
                return this;
            }
        }
    }
}

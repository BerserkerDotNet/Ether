using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;

namespace Ether.Vsts.Types
{
    public class VstsWorkItem : IWorkItem
    {
        private readonly WorkItemViewModel _workItem;

        public VstsWorkItem(WorkItemViewModel workItem)
        {
            _workItem = workItem;
            Updates = workItem.Updates?.Select(u => new VstsWorkItemUpdate(u));
        }

        public int Id
        {
            get
            {
                return _workItem.WorkItemId;
            }

            set
            {
                _workItem.WorkItemId = value;
            }
        }

        public string Title
        {
            get
            {
                return GetString(Constants.WorkItemTitleField);
            }

            set
            {
                Set(Constants.WorkItemTitleField, value);
            }
        }

        public string Type
        {
            get
            {
                return GetString(Constants.WorkItemTypeField);
            }

            set
            {
                Set(Constants.WorkItemTypeField, value);
            }
        }

        public IEnumerable<IWorkItemUpdate> Updates { get; }

        private string GetString(string key)
        {
            if (_workItem.Fields is object && _workItem.Fields.ContainsKey(key))
            {
                return _workItem.Fields[key];
            }

            return string.Empty;
        }

        private void Set(string key, string value)
        {
            if (_workItem.Fields is null)
            {
                _workItem.Fields = new System.Collections.Generic.Dictionary<string, string>();
            }

            if (_workItem.Fields.ContainsKey(key))
            {
                _workItem.Fields[key] = value;
            }
            else
            {
                _workItem.Fields.Add(key, value);
            }
        }
    }

    public class VstsWorkItemUpdate : IWorkItemUpdate
    {
        private readonly WorkItemUpdateViewModel _update;
        private readonly Regex _userParser = new Regex("(?<Name>[^<]+)\\s+<(?<Email>[^<>]+)>");

        public VstsWorkItemUpdate(WorkItemUpdateViewModel update)
        {
            _update = update;
        }

        public int Id => _update.Id;

        public int WorkItemId => _update.WorkItemId;

        public (string New, string Old) State => Get(Constants.WorkItemStateField);

        public (DateTime New, DateTime? Old) ChangedDate
        {
            get
            {
                var (@new, old) = Get(Constants.WorkItemChangedDateField);
                return (DateTime.Parse(@new), string.IsNullOrEmpty(old) ? (DateTime?)null : DateTime.Parse(old));
            }
        }

        public (UserReference New, UserReference Old) ResolvedBy
        {
            get
            {
                var (@new, old) = Get(Constants.WorkItemResolvedByField);
                return (ParseUser(@new), ParseUser(old));
            }
        }

        private UserReference ParseUser(string userString)
        {
            if (string.IsNullOrEmpty(userString))
            {
                return null;
            }

            var newUserMatch = _userParser.Match(userString);
            if (!newUserMatch.Success)
            {
                return null;
            }

            return new UserReference { Email = newUserMatch.Groups["Email"].Value, Title = newUserMatch.Groups["Name"].Value };
        }

        private (string New, string Old) Get(string key)
        {
            if (_update.Fields == null || !_update.Fields.ContainsKey(key))
            {
                return (string.Empty, string.Empty);
            }

            var field = _update.Fields[key];
            return (field.NewValue, field.OldValue);
        }
    }
}

using System;
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
}

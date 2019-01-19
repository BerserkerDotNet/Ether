using System;
using System.Collections.Generic;

namespace Ether.ViewModels
{
    public class WorkItemUpdateViewModel
    {
        public int Id { get; set; }

        public int WorkItemId { get; set; }

        public Dictionary<string, WorkItemFieldUpdate> Fields { get; set; }

        public WorkItemFieldUpdate this[string key]
        {
            get
            {
                if (Fields == null || !Fields.ContainsKey(key))
                {
                    return new WorkItemFieldUpdate();
                }

                return Fields[key];
            }
        }
    }
}

using System.Collections.Generic;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class WorkItemViewModel
    {
        public int WorkItemId { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public IEnumerable<WorkItemUpdateViewModel> Updates { get; set; }

        public IEnumerable<WorkItemRelation> Relations { get; set; }

        public string this[string key]
        {
            get
            {
                if (Fields == null || !Fields.ContainsKey(key))
                {
                    return string.Empty;
                }

                return Fields[key];
            }
        }

        public virtual bool Equals(WorkItemViewModel other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return WorkItemId == other.WorkItemId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WorkItemViewModel);
        }

        public override int GetHashCode()
        {
            return WorkItemId.GetHashCode();
        }
    }
}

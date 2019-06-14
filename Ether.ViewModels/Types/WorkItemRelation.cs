using System;
using System.Collections.Generic;

namespace Ether.ViewModels.Types
{
    public class WorkItemRelation
    {
        public Dictionary<string, string> Attributes { get; set; }

        public string RelationType { get; set; }

        public Uri Url { get; set; }
    }
}

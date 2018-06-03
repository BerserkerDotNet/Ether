using System;
using System.Collections.Generic;
using Ether.Core.Types;

namespace Ether.Core.Models.DTO
{
    public class Settings : BaseDto
    {
        public override Guid Id
        {
            get => Guid.Parse("a98f9ff6-efac-44ee-abfc-1d78d787d4d9");
        }

        public WorkItems WorkItemsSettings { get; set; }
        public Reports ReportsSettings { get; set; }
        public PullRequests PullRequestsSettings { get; set; }

        public class WorkItems
        {
            public bool DisableWorkitemsJob { get; set; }
            public TimeSpan? KeepLast { get; set; }
            public IEnumerable<Field> ETAFields { get; set; }
        }

        public class Field
        {
            public Field(string workItemType, string fieldName, ETAFieldType type)
            {
                WorkitemType = workItemType;
                FieldName = fieldName;
                FieldType = type;
            }

            public string WorkitemType { get; private set; }
            public string FieldName { get; private set; }

            public ETAFieldType FieldType { get; private set; }
        }

        public class PullRequests
        {
            public bool DisablePullRequestsJob { get; set; }
            public TimeSpan? KeepLast { get; set; }
        }

        public class Reports
        {
            public TimeSpan? KeepLast { get; set; }
        }
    }
}

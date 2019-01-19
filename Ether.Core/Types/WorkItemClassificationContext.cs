using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Core.Types
{
    public class WorkItemClassificationContext : IWorkItemClassificationContext
    {
        private static readonly DateTime MaxDate = new DateTime(9999, 1, 1);
        private readonly IEnumerable<IWorkItemsClassifier> _classifiers;

        public WorkItemClassificationContext(IEnumerable<IWorkItemsClassifier> classifiers)
        {
            _classifiers = classifiers;
        }

        public IEnumerable<WorkItemResolution> Classify(WorkItemViewModel item, ClassificationScope scope)
        {
            var rs = from c in _classifiers
                     let r = c.Classify(new WorkItemResolutionRequest { WorkItem = item, Team = scope.Team, StartDate = scope.StartDate, EndDate = scope.EndDate })
                     where !r.IsNone && (IsInRange(r, scope) || r.IsError)
                     select r;

            return rs.ToList();
        }

        private bool IsInRange(WorkItemResolution r, ClassificationScope scope)
        {
            return (r.ResolutionDate >= scope.StartDate && r.ResolutionDate <= scope.EndDate)
                || r.ResolutionDate == MaxDate;
        }
    }
}
using System;
using System.Collections.Generic;
using Ether.Core.Models;
using Ether.Core.Models.VSTS;
using Ether.Core.Interfaces;
using Ether.Core.Types;
using System.Linq;

namespace Ether.Core.Reporters.Classifiers
{
    public class WorkItemClassificationContext : IWorkItemClassificationContext
    {
        private static readonly DateTime VSTSMaxDate = new DateTime(9999, 1, 1);
        private readonly IEnumerable<IWorkItemsClassifier> _classifiers;

        public WorkItemClassificationContext(IEnumerable<IWorkItemsClassifier> classifiers)
        {
            _classifiers = classifiers;
        }

        public IEnumerable<WorkItemResolution> Classify(VSTSWorkItem item, ClassificationScope scope)
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
                || r.ResolutionDate == VSTSMaxDate;
        }
    }
}
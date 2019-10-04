using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
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

        public IEnumerable<IWorkItemEvent> Classify(WorkItemViewModel item, ClassificationScope scope)
        {
            var rs = from c in _classifiers
                     let resolutions = c.Classify(new WorkItemResolutionRequest { WorkItem = item, Team = scope.Team, StartDate = scope.StartDate, EndDate = scope.EndDate })
                     from r in resolutions
                     where IsInRange(r, scope) || IsError(r)
                     select r;

            return rs.ToList();
        }

        private bool IsError(IWorkItemEvent @event)
        {
            return @event is ErrorClassifyingWorkItemEvent;
        }

        private bool IsInRange(IWorkItemEvent r, ClassificationScope scope)
        {
            return (r.Date >= scope.StartDate && r.Date <= scope.EndDate)
                || r.Date == MaxDate;
        }
    }
}
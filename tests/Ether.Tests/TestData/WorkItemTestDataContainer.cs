using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Tests.TestData
{
    public class WorkItemTestDataContainer
    {
        private readonly IEnumerable<WorkItemTestData> _data;

        public WorkItemTestDataContainer(IEnumerable<WorkItemTestData> data)
        {
            _data = data;
        }

        public IEnumerable<WorkItemViewModel> WorkItems => _data.Select(d => d.WorkItem);

        public int ExpectedEstimatedToComplete => _data.Sum(d => d.ExpectedEstimatedToComplete);

        public int ExpectedDuration => _data.Sum(d => d.ExpectedDuration);

        public int ExpectedOriginalEstimate => _data.Sum(d => d.ExpectedOriginalEstimate);

        public WorkItemViewModel WorkItemById(int id) => _data.Single(w => w.WorkItem.WorkItemId == id).WorkItem;

        public IEnumerable<WorkItemResolution> ResolutionsById(int id) => _data.Single(w => w.WorkItem.WorkItemId == id).Resolutions;
    }
}

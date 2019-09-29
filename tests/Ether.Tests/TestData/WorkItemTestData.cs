using System.Collections.Generic;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.ViewModels;

namespace Ether.Tests.TestData
{
    public class WorkItemTestData
    {
        public WorkItemViewModel WorkItem { get; set; }

        public List<IWorkItemEvent> Resolutions { get; set; }

        public string Type { get; set; }

        public int ExpectedDuration { get; set; }

        public int ExpectedOriginalEstimate { get; set; }

        public int ExpectedEstimatedToComplete { get; set; }
    }
}

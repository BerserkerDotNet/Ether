using System;
using System.Collections.Generic;
using Ether.Contracts.Types;
using Ether.ViewModels;
using Ether.Vsts;

namespace Ether.Tests.Classifiers
{
    public class ExceptionWorkItemClassifier : BaseWorkItemsClassifier
    {
        public const string ExpectedReason = "Something went wrong!";

        public const string SupportedType = "Exception";

        protected override IEnumerable<WorkItemResolution> ClassifyInternal(WorkItemResolutionRequest request)
        {
            throw new Exception(ExpectedReason);
        }

        protected override bool IsSupported(WorkItemViewModel item)
        {
            return item.Fields[Constants.WorkItemTypeField] == SupportedType;
        }
    }
}

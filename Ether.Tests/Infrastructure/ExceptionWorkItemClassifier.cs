using Ether.Core.Models;
using Ether.Core.Reporters.Classifiers;
using System;

namespace Ether.Tests.Infrastructure
{
    public class ExceptionWorkItemClassifier : BaseWorkItemsClassifier
    {
        public const string ExpectedReason = "Something went wrong!";

        public const string SupportedType = "Exception";

        public ExceptionWorkItemClassifier()
            : base(SupportedType)
        {
        }

        protected override WorkItemResolution ClassifyInternal(WorkItemResolutionRequest request)
        {
            throw new Exception(ExpectedReason);
        }
    }
}

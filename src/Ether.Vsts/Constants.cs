namespace Ether.Vsts
{
    public static class Constants
    {
        public const string VstsType = "Vsts";

        #region Fields
        public const string WorkItemTitleField = "System.Title";
        public const string WorkItemStateField = "System.State";
        public const string WorkItemTagsField = "System.Tags";
        public const string WorkItemTypeField = "System.WorkItemType";
        public const string WorkItemReasonField = "System.Reason";
        public const string WorkItemAssignedToField = "System.AssignedTo";
        public const string WokItemCreatedDateField = "System.CreatedDate";
        public const string WorkItemAreaPathField = "System.AreaPath";
        public const string WorkItemResolvedByField = "Microsoft.VSTS.Common.ResolvedBy";
        public const string WorkItemClosedByField = "Microsoft.VSTS.Common.ClosedBy";
        public const string WorkItemChangedDateField = "System.ChangedDate";
        public const string OriginalEstimateField = "Microsoft.VSTS.Scheduling.OriginalEstimate";
        public const string RemainingWorkField = "Microsoft.VSTS.Scheduling.RemainingWork";
        public const string CompletedWorkField = "Microsoft.VSTS.Scheduling.CompletedWork";
        #endregion

        #region states
        public const string WorkItemStateNew = "New";
        public const string WorkItemStateActive = "Active";
        public const string WorkItemStateResolved = "Resolved";
        public const string WorkItemStateClosed = "Closed";
        #endregion

        public const string WorkItemTypeBug = "Bug";
        public const string WorkItemTypeTask = "Task";

        #region Tags
        public const string OnHoldTag = "onhold";
        public const string BlockedTag = "blocked";
        public const string CodeReviewTag = "codereview";
        #endregion
    }
}

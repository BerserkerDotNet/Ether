using Autofac;
using Ether.Contracts.Types;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Config
{
    public class VstsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var workItemIndex = new DbIndexDescriptor(typeof(WorkItem), nameof(WorkItem.WorkItemId), isAscending: true);
            var pullRequestIndex = new DbIndexDescriptor(typeof(PullRequest), nameof(PullRequest.PullRequestId), isAscending: true);

            builder.RegisterInstance(workItemIndex);
            builder.RegisterInstance(pullRequestIndex);

            base.Load(builder);
        }
    }
}

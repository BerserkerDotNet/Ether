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
            var workItemClassRegistration = new ClassMapRegistration(typeof(WorkItem));
            var pullRequestClassRegistration = new ClassMapRegistration(typeof(PullRequest));

            builder.RegisterInstance(workItemIndex);
            builder.RegisterInstance(pullRequestIndex);
            builder.RegisterInstance(workItemClassRegistration);
            builder.RegisterInstance(pullRequestClassRegistration);

            base.Load(builder);
        }
    }
}

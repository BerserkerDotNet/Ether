using Autofac;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;
using Ether.Vsts.Dto;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Jobs;
using Ether.Vsts.Types;

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
            builder.RegisterType<VstsInitialMigration>().As<IMigration>();
            builder.RegisterType<VstsClientFactory>().As<IVstsClientFactory>();
            builder.RegisterType<PullRequestsSyncJob>().As<IJob>();

            base.Load(builder);
        }
    }
}

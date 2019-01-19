using System;
using System.Linq;
using Autofac;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Data;
using Ether.Core.Types;

namespace Ether.Core.Config
{
    public class EtherCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Ether.")).ToArray();
            builder.RegisterType<DefaultMediator>().As<IMediator>();
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IQueryHandler<,>))
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(ICommandHandler<,>))
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .AsImplementedInterfaces();

            builder.RegisterType<MongoDbConfigurator>()
                .As<IDbConfigurator>()
                .SingleInstance();
            builder.RegisterType<MongoRepository>()
                .As<IRepository>()
                .SingleInstance();

            builder.RegisterType<WorkItemClassificationContext>().As<IWorkItemClassificationContext>();

            base.Load(builder);
        }
    }
}

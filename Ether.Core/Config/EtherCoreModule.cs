using System;
using System.Linq;
using Autofac;
using Ether.Contracts.Interfaces.CQS;
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

            base.Load(builder);
        }
    }
}

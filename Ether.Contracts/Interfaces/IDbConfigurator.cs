using System.Collections.Generic;
using Ether.Contracts.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IDbConfigurator
    {
        IEnumerable<ClassMapRegistration> Registrations { get; }

        IEnumerable<DbIndexDescriptor> Indexes { get; }
    }
}

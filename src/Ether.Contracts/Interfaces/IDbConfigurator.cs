using System.Collections.Generic;
using Ether.Contracts.Types;

namespace Ether.Contracts.Interfaces
{
    public interface IDbConfigurator
    {
        IEnumerable<IClassMapRegistration> Registrations { get; }

        IEnumerable<DbIndexDescriptor> Indexes { get; }
    }
}

using System;
using System.Collections.Generic;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types;

namespace Ether.Core.Data
{
    public class MongoDbConfigurator : IDbConfigurator
    {
        public MongoDbConfigurator(IEnumerable<ClassMapRegistration> typesToRegister, IEnumerable<DbIndexDescriptor> indexes)
        {
            Registrations = typesToRegister;
            Indexes = indexes;
        }

        public IEnumerable<ClassMapRegistration> Registrations { get; }

        public IEnumerable<DbIndexDescriptor> Indexes { get; }
    }
}

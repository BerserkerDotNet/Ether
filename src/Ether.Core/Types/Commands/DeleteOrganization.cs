using System;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Core.Types.Commands
{
    public class DeleteOrganization : ICommand
    {
        public Guid Id { get; set; }
    }
}

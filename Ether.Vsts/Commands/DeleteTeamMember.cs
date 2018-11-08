using System;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Commands
{
    public class DeleteTeamMember : ICommand
    {
        public Guid Id { get; set; }
    }
}

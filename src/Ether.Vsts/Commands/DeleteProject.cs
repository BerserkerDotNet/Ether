using System;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Commands
{
    public class DeleteProject : ICommand
    {
        public Guid Id { get; set; }
    }
}

using System;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveProjectHandler : SaveHandler<Project, SaveProject>
    {
        public SaveProjectHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override void ValidateCommand(SaveProject command)
        {
            if (command.Project == null)
            {
                throw new ArgumentNullException(nameof(command.Project));
            }
        }

        protected override object GetData(SaveProject command) => command.Project;
    }
}

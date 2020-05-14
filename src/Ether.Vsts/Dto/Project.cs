using System;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    public class Project : BaseDto
    {
        public string Name { get; set; }

        public bool IsWorkItemsEnabled { get; set; }

        public Guid? Organization { get; set; }

        public Guid? Identity { get; set; }
    }
}

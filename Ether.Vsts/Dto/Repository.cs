using System;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    public class Repository : BaseDto
    {
        public string Name { get; set; }

        public Guid Project { get; set; }
    }
}

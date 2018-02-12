using System;

namespace Ether.Core.Models.DTO
{
    public class Query : BaseDto
    {
        public string Name { get; set; }

        public Guid QueryId { get; set; }

        public Guid Project { get; set; }
    }
}

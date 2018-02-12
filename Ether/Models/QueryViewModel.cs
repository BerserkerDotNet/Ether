using System;

namespace Ether.Models
{
    public class QueryViewModel
    {
        public Guid Id { get; set; }
        public Guid? QueryId { get; set; }
        public string Name { get; set; }
        public Guid Project { get; set; }
    }
}

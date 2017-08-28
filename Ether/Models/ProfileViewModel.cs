using System;
using System.Collections.Generic;

namespace Ether.Models
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Guid> Repositories { get; set; }
        public IEnumerable<Guid> Members { get; set; }
    }
}
using Ether.Types.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Models
{
    public class ProfileListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<VSTSRepository> Repositories { get; set; }
        public IEnumerable<TeamMember> Members { get; set; }
        public IEnumerable<VSTSProject> Projects { get; set; }

        public VSTSProject GetFor(VSTSRepository repo)
        {
            return Projects.SingleOrDefault(p => p.Id == repo.Project);
        }
    }
}

using System;

namespace Ether.Core.Models.VSTS
{
    public class PullRequestIteration
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public VSTSUser Author { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
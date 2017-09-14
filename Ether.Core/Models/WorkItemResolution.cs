using System;

namespace Ether.Core.Models
{
    public class WorkItemResolution
    {
        public const string ReasonInvestigated = "Investigated";
        private const string NoneValue = "None";

        public WorkItemResolution(string resolution, string reason, DateTime resolutionDate, string teamMember)
        {
            Resolution = resolution;
            Reason = reason;
            ResolutionDate = resolutionDate;
            TeamMember = teamMember;
        }

        public string Resolution { get; }
        public string Reason { get; }
        public DateTime ResolutionDate { get; }
        public string TeamMember { get;}

        public bool IsNone => string.Equals(Resolution, NoneValue) && string.Equals(Reason, NoneValue);

        public static WorkItemResolution None => new WorkItemResolution(NoneValue, NoneValue, DateTime.MinValue, NoneValue);
    }
}

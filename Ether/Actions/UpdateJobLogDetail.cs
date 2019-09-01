using System;
using Ether.Redux.Interfaces;
using Ether.ViewModels.Types;

namespace Ether.Actions
{
    public class UpdateJobLogDetail : IAction
    {
        public Guid JobLogId { get; set; }

        public JobDetails Details { get; set; }
    }
}

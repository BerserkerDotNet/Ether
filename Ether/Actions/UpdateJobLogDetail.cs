using Ether.Redux.Interfaces;
using Ether.ViewModels.Types;
using System;

namespace Ether.Actions
{
    public class UpdateJobLogDetail : IAction
    {
        public Guid JobLogId { get; set; }

        public JobDetails Details { get; set; }
    }
}

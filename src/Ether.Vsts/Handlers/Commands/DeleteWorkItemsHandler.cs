using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using static Ether.Contracts.Types.NullUtil;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteWorkItemsHandler : ICommandHandler<DeleteWorkItems>
    {
        private readonly IRepository _repository;

        public DeleteWorkItemsHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteWorkItems command)
        {
            CheckIfArgumentNull(command.Ids, nameof(command.Ids));

            if (!command.Ids.Any())
            {
                return;
            }

            foreach (var id in command.Ids)
            {
                await _repository.DeleteAsync<WorkItem>(workitem => workitem.WorkItemId == id);
            }

            var idsInStrings = command.Ids.Select(id => id.ToString());
            var teamMembers = await _repository.GetByFilteredArrayAsync<TeamMember>("RelatedWorkItems", idsInStrings.ToArray());

            foreach (var teamMember in teamMembers)
            {
                var remainingIds = teamMember.RelatedWorkItems
                    .Except(command.Ids)
                    .ToArray();

                await _repository.UpdateFieldValue(teamMember, m => m.RelatedWorkItems, remainingIds);
            }
        }
    }
}

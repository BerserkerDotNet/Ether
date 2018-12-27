using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using static Ether.Contracts.Types.NullUtil;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveWorkItemsForUserHandler : ICommandHandler<SaveWorkItemsForUser>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveWorkItemsForUserHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SaveWorkItemsForUser command)
        {
            CheckIfArgumentNull(command.Member, nameof(command.Member));
            CheckIfArgumentNull(command.Workitems, nameof(command.Workitems));

            if (!command.Workitems.Any())
            {
                return;
            }

            var relatedIds = command.Workitems.Select(w => w.WorkitemId).ToArray();
            await _repository.UpdateFieldValue(new TeamMember { Id = command.Member.Id }, m => m.RelatedWorkItems, relatedIds);
            foreach (var workItemModel in command.Workitems)
            {
                var workItem = new WorkItem { CurrentState = workItemModel.WorkItem, Updates = workItemModel.Updates, WorkItemId = workItemModel.WorkitemId };
                await _repository.CreateOrUpdateAsync(workItem, w => w.CurrentState.Id == workItemModel.WorkitemId);
            }
        }
    }
}

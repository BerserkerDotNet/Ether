using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Microsoft.Extensions.Logging;
using static Ether.Contracts.Types.NullUtil;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveWorkItemsForUserHandler : ICommandHandler<SaveWorkItemsForUser>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SaveWorkItemsForUserHandler> _logger;

        public SaveWorkItemsForUserHandler(IRepository repository, IMapper mapper, ILogger<SaveWorkItemsForUserHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Handle(SaveWorkItemsForUser command)
        {
            CheckIfArgumentNull(command.Member, nameof(command.Member));
            CheckIfArgumentNull(command.Workitems, nameof(command.Workitems));

            if (!command.Workitems.Any())
            {
                return;
            }

            var member = await _repository.GetSingleAsync<TeamMember>(command.Member.Id);
            member.RelatedWorkItems = member.RelatedWorkItems ?? new int[0];
            var relatedIds = member.RelatedWorkItems
                .Union(command.Workitems.Select(w => w.WorkItemId))
                .Distinct()
                .ToArray();
            await _repository.UpdateFieldValue(member, m => m.RelatedWorkItems, relatedIds);
            await _repository.UpdateFieldValue(member, m => m.LastWorkitemsFetchDate, DateTime.UtcNow);
            foreach (var workItemModel in command.Workitems)
            {
                var workItem = _mapper.Map<WorkItem>(workItemModel);
                try
                {
                    await _repository.CreateOrUpdateIfAsync(w => w.WorkItemId == workItemModel.WorkItemId, workItem);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error saving workitem {WorkItemId}.", workItem.WorkItemId);
                }
            }
        }
    }
}

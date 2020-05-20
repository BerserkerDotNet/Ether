using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class DeleteReportHandler : ICommandHandler<DeleteReport>
    {
        private readonly IRepository _repository;

        public DeleteReportHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteReport input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            await _repository.DeleteAsync<ReportResult>(input.Id);
        }
    }
}

using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetAllJobLogs : GetAllPagedQuery<JobLogViewModel>
    {
        public GetAllJobLogs(int page, int itemsPerPage = 10)
        {
            Page = page;
            ItemsPerPage = itemsPerPage;
        }
    }
}

namespace Ether.Actions.Async
{
    public class FetchJobLogsCommand
    {
        public FetchJobLogsCommand(int page = 1, int itemsPerPage = 10)
        {
            Page = page;
            ItemsPerPage = itemsPerPage;
        }

        public int Page { get; }

        public int ItemsPerPage { get; }
    }
}

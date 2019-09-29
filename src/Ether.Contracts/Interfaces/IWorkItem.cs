namespace Ether.Contracts.Interfaces
{
    public interface IWorkItem
    {
        int Id { get; set; }

        string Title { get; set; }

        string Type { get; set; }
    }
}

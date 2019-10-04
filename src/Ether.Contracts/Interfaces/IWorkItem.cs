using System.Collections.Generic;

namespace Ether.Contracts.Interfaces
{
    public interface IWorkItem
    {
        int Id { get; set; }

        string Title { get; set; }

        string Type { get; set; }

        IEnumerable<IWorkItemUpdate> Updates { get; }
    }
}

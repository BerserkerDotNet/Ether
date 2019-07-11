using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.EditableTable
{
    public class NoOpEditableTableDataProvider : IEditableTableDataProvider
    {
        public Task Delete<T>(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> Load<T>()
        {
            return Task.FromResult(Enumerable.Empty<T>());
        }

        public Task Save<T>(T item)
        {
            return Task.CompletedTask;
        }
    }
}

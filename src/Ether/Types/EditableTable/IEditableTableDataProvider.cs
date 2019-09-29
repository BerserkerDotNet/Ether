using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Types.EditableTable
{
    public interface IEditableTableDataProvider
    {
        Task<IEnumerable<T>> Load<T>();

        Task Save<T>(T item);

        Task Delete<T>(Guid id);
    }
}

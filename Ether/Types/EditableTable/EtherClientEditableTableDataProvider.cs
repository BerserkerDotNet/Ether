using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Types.EditableTable
{
    public class EtherClientEditableTableDataProvider : IEditableTableDataProvider
    {
        public EtherClientEditableTableDataProvider(EtherClient client)
        {
            Client = client;
        }

        protected EtherClient Client { get; private set; }

        public virtual async Task Delete<T>(Guid id)
        {
            await Client.Delete<T>(id);
        }

        public virtual Task<IEnumerable<T>> Load<T>()
        {
            return Client.GetAll<T>();
        }

        public virtual async Task Save<T>(T item)
        {
            await Client.Save(item);
        }
    }
}

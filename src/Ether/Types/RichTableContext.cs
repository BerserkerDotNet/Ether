using Ether.Types.Interfaces;

namespace Ether.Types
{
    public class RichTableContext<T>
    {
        private readonly IRichTable<T> _table;

        public RichTableContext(T item, IRichTable<T> table)
        {
            _table = table;
            CurrentItem = item;
        }

        public T CurrentItem { get; private set; }

        public void Edit()
        {
            _table.Edit(CurrentItem);
        }
    }
}

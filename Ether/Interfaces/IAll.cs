using System.Collections.Generic;

namespace Ether.Interfaces
{
    public interface IAll<T>
    {
        IEnumerable<T> Value { get; }
    }
}

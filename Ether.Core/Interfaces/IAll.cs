using System.Collections.Generic;

namespace Ether.Core.Interfaces
{
    public interface IAll<T>
    {
        IEnumerable<T> Value { get; }
    }
}

using System;

namespace Ether.Contracts.Types
{
    public class DbIndexDescriptor
    {
        public DbIndexDescriptor(Type documentType, string field, bool isAscending)
        {
            DocumentType = documentType;
            Field = field;
            IsAscending = isAscending;
        }

        public string Field { get; }

        public Type DocumentType { get; }

        public bool IsAscending { get; }
    }
}

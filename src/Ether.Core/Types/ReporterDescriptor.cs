using System;

namespace Ether.Core.Types
{
    public class ReporterDescriptor
    {
        public ReporterDescriptor(string uniqueName, Type commandType, Type dtoType, Type viewModelType, string displayName)
        {
            UniqueName = uniqueName;
            CommandType = commandType;
            DtoType = dtoType;
            ViewModelType = viewModelType;
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }

        public Type CommandType { get; set; }

        public Type DtoType { get; set; }

        public Type ViewModelType { get; set; }

        public string UniqueName { get; set; }
    }
}
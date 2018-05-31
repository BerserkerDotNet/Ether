using System;

namespace Ether.Core.Types.Exceptions
{
    public class MissingETASettingsException : Exception
    {
    }

    public class IncompleteETASettingsException : Exception
    {
        public IncompleteETASettingsException(string workitemType, string fieldType)
            : base($"Couldn't find ETA field of type '{fieldType}' for '{workitemType}'")
        {
        }
    }
}
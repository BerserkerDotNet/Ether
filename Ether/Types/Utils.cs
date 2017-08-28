using System;

namespace Ether.Types
{
    public static class Utils
    {
        public static string GetNameForAllItems(Type modelType)
        {
            return $"All{modelType.Name}s";
        }
    }
}

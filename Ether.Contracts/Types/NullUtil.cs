using System;

namespace Ether.Contracts.Types
{
    public static class NullUtil
    {
        public static void CheckIfArgumentNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}

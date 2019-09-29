using System;

namespace Ether.Vsts.Exceptions
{
    [Serializable]
    public class IdentityNotFoundException : Exception
    {
        public IdentityNotFoundException()
        {
        }

        public IdentityNotFoundException(string message)
            : base(message)
        {
        }

        public IdentityNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected IdentityNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}

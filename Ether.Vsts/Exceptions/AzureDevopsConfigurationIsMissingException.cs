using System;

namespace Ether.Vsts.Exceptions
{
    [Serializable]
    public class AzureDevopsConfigurationIsMissingException : Exception
    {
        public AzureDevopsConfigurationIsMissingException()
        {
        }

        public AzureDevopsConfigurationIsMissingException(string message)
            : base(message)
        {
        }

        public AzureDevopsConfigurationIsMissingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AzureDevopsConfigurationIsMissingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}

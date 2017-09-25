namespace Ether.Core.Configuration
{
    public class VSTSConfiguration
    {
        public string InstanceName { get; set; }

        public string AccessToken { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(InstanceName) && !string.IsNullOrEmpty(AccessToken);
    }
}

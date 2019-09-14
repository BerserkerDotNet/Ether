namespace Ether.Redux.Blazor
{
    public class DevToolsMessage
    {
        public string Type { get; set; }

        public string Source { get; set; }

        public string State { get; set; }

        public DevToolsPayload Payload { get; set; }
    }
}

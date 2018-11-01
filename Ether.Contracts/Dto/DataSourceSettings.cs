namespace Ether.Contracts.Dto
{
    public class DataSourceSettings : BaseDto
    {
        public DataSourceSettings(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }
}

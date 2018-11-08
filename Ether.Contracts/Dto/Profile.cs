namespace Ether.Contracts.Dto
{
    public class Profile : BaseDto
    {
        public Profile(string type)
        {
            Type = type;
        }

        public string Name { get; set; }

        public string Type { get; private set; }
    }
}

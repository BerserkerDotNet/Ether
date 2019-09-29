namespace Ether.Contracts.Dto
{
    public class User : BaseDto
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string[] Roles { get; set; }
    }
}
